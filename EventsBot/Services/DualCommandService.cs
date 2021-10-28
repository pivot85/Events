using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Bot
{
    public interface ParserErrorHandler
    {
        Task ExecuteAsync(DualCommandContext context, Discord.Commands.ParameterInfo paramType, object input);
    }

    public class ParseErrorHandlerAttribute : Attribute
    {
        internal readonly ParserErrorHandler Handler;

        public ParseErrorHandlerAttribute(Type handler)
        {
            if (!handler.IsAssignableTo(typeof(ParserErrorHandler)))
                throw new ArgumentException();

            Handler = (ParserErrorHandler)Activator.CreateInstance(handler);
        }
    }

    public class DualCommandService
    {
        public event Func<LogMessage, Task> Log
        {
            add => _underlyingService.Log += value;
            remove => _underlyingService.Log -= value;
        }
        public event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted
        {
            add => _underlyingService.CommandExecuted += value;
            remove => _underlyingService.CommandExecuted -= value;
        }

        public readonly ConcurrentDictionary<Type, TypeReader> DefaultTypeReaders;
        public readonly ImmutableList<(Type EntityType, Type TypeReaderType)> EntityTypeReaders;

        private List<ModuleInfo> CustomModules = new List<ModuleInfo>();
        private CommandService _underlyingService;
        private static readonly TypeInfo _moduleTypeInfo = typeof(DualModuleBase).GetTypeInfo();
        private readonly SemaphoreSlim _moduleLock;
        private CommandServiceConfig _config;
        private Logger _log;

        /// <summary>
        ///     Creates a new instance of the command service
        /// </summary>
        public DualCommandService()
            : this(new CommandServiceConfig())
        {
        }

        /// <summary>
        ///     Creates a new instance of the command service
        /// </summary>
        /// <param name="conf">The config for the command service</param>
        public DualCommandService(CommandServiceConfig conf)
        {
            _log = Logger.GetLogger<DualCommandService>();
            conf.IgnoreExtraArgs = true;
            _moduleLock = new SemaphoreSlim(1, 1);
            _underlyingService = new CommandService(conf);
            this._config = conf;
            DefaultTypeReaders = new ConcurrentDictionary<Type, TypeReader>();
            foreach (var type in PrimitiveParsers.SupportedTypes)
            {
                DefaultTypeReaders[type] = PrimitiveTypeReader.Create(type);
                DefaultTypeReaders[typeof(Nullable<>).MakeGenericType(type)] = NullableTypeReader.Create(type, DefaultTypeReaders[type]);
            }

            var entityTypeReaders = ImmutableList.CreateBuilder<(Type, Type)>();
            entityTypeReaders.Add((typeof(IMessage), typeof(MessageTypeReader<>)));
            entityTypeReaders.Add((typeof(IChannel), typeof(ChannelTypeReader<>)));
            entityTypeReaders.Add((typeof(IRole), typeof(RoleTypeReader<>)));
            entityTypeReaders.Add((typeof(IUser), typeof(UserTypeReader<>)));
        }

        /// <summary>
        ///     Adds a type reader to the service.
        /// </summary>
        /// <typeparam name="T">The parameter type for the reader to read.</typeparam>
        /// <param name="reader">The reader to read the parameter type.</param>
        public void AddTypeReader<T>(TypeReader reader)
           => AddTypeReader(typeof(T), reader);

        /// <summary>
        ///     Adds a type reader to the service.
        /// </summary>
        /// <param name="type">The parameter type for the reader to read.</param>
        /// <param name="reader">The reader to read the parameter type.</param>
        public void AddTypeReader(Type type, TypeReader reader)
            => _underlyingService.AddTypeReader(type, reader);

        /// <summary>
        ///     Gets a type reader for the specififed type.
        /// </summary>
        /// <param name="type">The type to get the reader from.</param>
        /// <returns>A type reader for the given type if found; otherwise <see langword="null"/>.</returns>
        public TypeReader GetTypeReader(Type type)
        {
            var customReader = _underlyingService.TypeReaders.FirstOrDefault(x => x.Key == type);

            if (customReader != null)
                return customReader.FirstOrDefault();

            if (DefaultTypeReaders.TryGetValue(type, out var reader))
                return reader;
            var typeInfo = type.GetTypeInfo();

            //Is this an enum?
            if (typeInfo.IsEnum)
            {
                reader = EnumTypeReader.GetReader(type);
                DefaultTypeReaders[type] = reader;
                return reader;
            }

            //Is this an entity?
            for (int i = 0; i < EntityTypeReaders.Count; i++)
            {
                if (type == EntityTypeReaders[i].EntityType || typeInfo.ImplementedInterfaces.Contains(EntityTypeReaders[i].EntityType))
                {
                    reader = Activator.CreateInstance(EntityTypeReaders[i].TypeReaderType.MakeGenericType(type)) as TypeReader;
                    DefaultTypeReaders[type] = reader;
                    return reader;
                }
            }

            return null;
        }

        /// <summary>
        ///     Executes a command.
        /// </summary>
        /// <param name="command">The slash command to execute from.</param>
        /// <param name="context">The context for the module.</param>
        /// <param name="provider">The service provider for di.</param>
        /// <param name="multiMatchHandling">The handling mode when multiple command matches are found.</param>
        /// <returns>
        ///     A <see cref="IResult"/> containing the result of the execution.
        /// </returns>
        public Task<IResult> ExecuteAsync(SocketSlashCommand command, ICommandContext context, IServiceProvider provider, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            var name = command.CommandName;

            if (command.Data.Options?.Count == 1 && command.Data.Options?.First().Type == Discord.ApplicationCommandOptionType.SubCommand)
            {
                name += " " + GetSubName(command.Data.Options.First());
            }

            return ExecuteAsync(context, name, provider);
        }

        private string GetSubName(SocketSlashCommandDataOption opt)
        {
            if (opt == null)
                return "";

            if (opt.Type == Discord.ApplicationCommandOptionType.SubCommand)
            {
                var others = GetSubName(opt.Options?.FirstOrDefault());

                return opt.Name + " " + others;
            }

            return "";
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name="context">The context of the command.</param>
        /// <param name="argPos">The position of which the command starts at.</param>
        /// <param name="services">The service to be used in the command's dependency injection.</param>
        /// <param name="multiMatchHandling">The handling mode when multiple command matches are found.</param>
        /// <returns>
        ///     A task that represents the asynchronous execution operation. The task result contains the result of the
        ///     command execution.
        /// </returns>
        public Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
            => ExecuteAsync(context, context.Message.Content.Substring(argPos), services, multiMatchHandling);

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name="context">The context of the command.</param>
        /// <param name="input">The command string.</param>
        /// <param name="services">The service to be used in the command's dependency injection.</param>
        /// <param name="multiMatchHandling">The handling mode when multiple command matches are found.</param>
        /// <returns>
        ///     A task that represents the asynchronous execution operation. The task result contains the result of the
        ///     command execution.
        /// </returns>
        public Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
            => _underlyingService.ExecuteAsync(context, input, services, multiMatchHandling);

        /// <summary>
        ///     Registers the modules in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly containing the modules to register.</param>
        /// <param name="services">A service provider for DI.</param>
        public async Task RegisterModulesAsync(Assembly assembly, IServiceProvider services)
        {
            await _moduleLock.WaitAsync().ConfigureAwait(false);

            var types = Search(assembly);
            await BuildAsync(types, services).ConfigureAwait(false);
        }

        private async Task<Dictionary<Type, ModuleInfo>> BuildAsync(IEnumerable<TypeInfo> validTypes, IServiceProvider services)
        {
            var topLevelGroups = validTypes.Where(x => x.DeclaringType == null || !IsValidModuleDefinition(x.DeclaringType.GetTypeInfo()));

            var result = new Dictionary<Type, ModuleInfo>();

            foreach (var typeInfo in topLevelGroups)
            {
                // TODO: This shouldn't be the case; may be safe to remove?
                if (result.ContainsKey(typeInfo.AsType()))
                    continue;

                ModuleInfo module = null;

                if (_moduleTypeInfo.IsAssignableFrom(typeInfo))
                {
                    module = await _underlyingService.CreateModuleAsync("", (x) => BuildModule(x, typeInfo, services));
                    CustomModules.Add(module);
                }
                else
                {
                    module = await _underlyingService.AddModuleAsync(typeInfo, services).ConfigureAwait(false);
                }

                result.TryAdd(typeInfo.AsType(), module);
            }

            _log.Debug($"Successfully built {result.Count} modules.", Severity.CommandService);

            return result;
        }

        private IReadOnlyList<TypeInfo> Search(Assembly assembly)
        {
            bool IsLoadableModule(TypeInfo info)
            {
                return info.DeclaredMethods.Any(x => x.GetCustomAttribute<CommandAttribute>() != null) &&
                    info.GetCustomAttribute<DontAutoLoadAttribute>() == null;
            }

            var result = new List<TypeInfo>();

            foreach (var typeInfo in assembly.DefinedTypes)
            {
                if (typeInfo.IsPublic || typeInfo.IsNestedPublic)
                {
                    if (IsValidModuleDefinition(typeInfo) &&
                        !typeInfo.IsDefined(typeof(DontAutoLoadAttribute)))
                    {
                        result.Add(typeInfo);
                    }
                }
                else if (IsLoadableModule(typeInfo))
                {
                    _log.Warn($"Class {typeInfo.FullName} is not public and cannot be loaded. To suppress this message, mark the class with {nameof(DontAutoLoadAttribute)}.");
                }
            }

            return result;
        }

        private static bool IsValidModuleDefinition(TypeInfo typeInfo)
        {
            return (_moduleTypeInfo.IsAssignableFrom(typeInfo) || IsSubclassOfRawGeneric(typeof(ModuleBase<>), typeInfo.AsType())) &&
                   !typeInfo.IsAbstract &&
                   !typeInfo.ContainsGenericParameters;
        }
        private static bool IsValidCommandDefinition(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(CommandAttribute)) &&
                   (methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType == typeof(Task<RuntimeResult>)) &&
                   !methodInfo.IsStatic &&
                   !methodInfo.IsGenericMethod;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private void BuildModule(ModuleBuilder builder, TypeInfo typeInfo, IServiceProvider services)
        {
            var attributes = typeInfo.GetCustomAttributes();

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case NameAttribute name:
                        builder.Name = name.Text;
                        break;
                    case SummaryAttribute summary:
                        builder.Summary = summary.Text;
                        break;
                    case RemarksAttribute remarks:
                        builder.Remarks = remarks.Text;
                        break;
                    case AliasAttribute alias:
                        builder.AddAliases(alias.Aliases);
                        break;
                    case GroupAttribute group:
                        builder.Name = builder.Name ?? group.Prefix;
                        builder.Group = group.Prefix;
                        builder.AddAliases(group.Prefix);
                        break;
                    case PreconditionAttribute precondition:
                        builder.AddPrecondition(precondition);
                        break;
                    default:
                        builder.AddAttributes(attribute);
                        break;
                }
            }

            //Check for unspecified info
            if (builder.Aliases.Count == 0)
                builder.AddAliases("");
            if (builder.Name == null)
                builder.Name = typeInfo.Name;

            // Get all methods (including from inherited members), that are valid commands
            var validCommands = typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsValidCommandDefinition);

            foreach (var method in validCommands)
            {
                var name = method.GetCustomAttribute<CommandAttribute>();

                var createInstance = ReflectionUtils.CreateBuilder<DualModuleBase>(typeInfo);

                async Task<IResult> ExecuteCallback(ICommandContext context, object[] args, IServiceProvider services, CommandInfo cmd)
                {
                    var instance = createInstance(services);
                    instance.SetContext(context);
                    args = await instance.CreateInteractionArgsAsync(cmd, this, args);

                    if (args == null)
                        return ParseResult.FromError(CommandError.ParseFailed, "Input was not in the correct format");

                    try
                    {
                        await instance.BeforeExecuteAsync().ConfigureAwait(false);

                        var task = method.Invoke(instance, args) as Task ?? Task.Delay(0);
                        if (task is Task<RuntimeResult> resultTask)
                        {
                            return await resultTask.ConfigureAwait(false);
                        }
                        else
                        {
                            await task.ConfigureAwait(false);
                            return ExecuteResult.FromSuccess();
                        }
                    }
                    finally
                    {
                        (instance as IDisposable)?.Dispose();
                    }
                }

                builder.AddCommand(name.Text, ExecuteCallback, (command) =>
                {
                    BuildCommand(command, method, services);
                });
            }
        }

        private void BuildCommand(CommandBuilder builder, MethodInfo method, IServiceProvider serviceprovider)
        {
            var attributes = method.GetCustomAttributes();

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case CommandAttribute command:
                        builder.AddAliases(command.Text);
                        builder.RunMode = command.RunMode;
                        builder.Name = builder.Name ?? command.Text;
                        builder.IgnoreExtraArgs = command.IgnoreExtraArgs ?? _config.IgnoreExtraArgs;
                        break;
                    case NameAttribute name:
                        builder.Name = name.Text;
                        break;
                    case PriorityAttribute priority:
                        builder.Priority = priority.Priority;
                        break;
                    case SummaryAttribute summary:
                        builder.Summary = summary.Text;
                        break;
                    case RemarksAttribute remarks:
                        builder.Remarks = remarks.Text;
                        break;
                    case AliasAttribute alias:
                        builder.AddAliases(alias.Aliases);
                        break;
                    case PreconditionAttribute precondition:
                        builder.AddPrecondition(precondition);
                        break;
                    default:
                        builder.AddAttributes(attribute);
                        break;
                }
            }

            if (builder.Name == null)
                builder.Name = method.Name;

            var parameters = method.GetParameters();
            int pos = 0, count = parameters.Length;
            foreach (var paramInfo in parameters)
            {
                builder.AddParameter(paramInfo.Name, paramInfo.ParameterType, (parameter) =>
                {
                    BuildParameter(parameter, paramInfo, pos++, count, serviceprovider);
                });
            }
        }

        private void BuildParameter(ParameterBuilder builder, System.Reflection.ParameterInfo paramInfo, int position, int count, IServiceProvider services)
        {
            var attributes = paramInfo.GetCustomAttributes();
            var paramType = paramInfo.ParameterType;

            builder.IsOptional = true;
            builder.DefaultValue = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : null;

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case SummaryAttribute summary:
                        builder.Summary = summary.Text;
                        break;
                    case ParamArrayAttribute _:
                        builder.IsMultiple = true;
                        paramType = paramType.GetElementType();
                        break;
                    case ParameterPreconditionAttribute precon:
                        builder.AddPrecondition(precon);
                        break;
                    case RemainderAttribute _:
                        if (position != count - 1)
                            throw new InvalidOperationException($"Remainder parameters must be the last parameter in a command. Parameter: {paramInfo.Name} in {paramInfo.Member.DeclaringType.Name}.{paramInfo.Member.Name}");

                        builder.IsRemainder = true;
                        break;
                    default:
                        builder.AddAttributes(attribute);
                        break;
                }
            }
        }
    }

    internal delegate bool TryParseDelegate<T>(string str, out T value);

    internal static class PrimitiveParsers
    {
        private static readonly Lazy<IReadOnlyDictionary<Type, Delegate>> Parsers = new Lazy<IReadOnlyDictionary<Type, Delegate>>(CreateParsers);

        public static IEnumerable<Type> SupportedTypes = Parsers.Value.Keys;

        static IReadOnlyDictionary<Type, Delegate> CreateParsers()
        {
            var parserBuilder = ImmutableDictionary.CreateBuilder<Type, Delegate>();
            parserBuilder[typeof(bool)] = (TryParseDelegate<bool>)bool.TryParse;
            parserBuilder[typeof(sbyte)] = (TryParseDelegate<sbyte>)sbyte.TryParse;
            parserBuilder[typeof(byte)] = (TryParseDelegate<byte>)byte.TryParse;
            parserBuilder[typeof(short)] = (TryParseDelegate<short>)short.TryParse;
            parserBuilder[typeof(ushort)] = (TryParseDelegate<ushort>)ushort.TryParse;
            parserBuilder[typeof(int)] = (TryParseDelegate<int>)int.TryParse;
            parserBuilder[typeof(uint)] = (TryParseDelegate<uint>)uint.TryParse;
            parserBuilder[typeof(long)] = (TryParseDelegate<long>)long.TryParse;
            parserBuilder[typeof(ulong)] = (TryParseDelegate<ulong>)ulong.TryParse;
            parserBuilder[typeof(float)] = (TryParseDelegate<float>)float.TryParse;
            parserBuilder[typeof(double)] = (TryParseDelegate<double>)double.TryParse;
            parserBuilder[typeof(decimal)] = (TryParseDelegate<decimal>)decimal.TryParse;
            parserBuilder[typeof(DateTime)] = (TryParseDelegate<DateTime>)TryParseDate;
            parserBuilder[typeof(DateTimeOffset)] = (TryParseDelegate<DateTimeOffset>)DateTimeOffset.TryParse;
            //parserBuilder[typeof(TimeSpan)] = (TryParseDelegate<TimeSpan>)TimeSpan.TryParse;
            parserBuilder[typeof(char)] = (TryParseDelegate<char>)char.TryParse;
            return parserBuilder.ToImmutable();
        }

        public static TryParseDelegate<T> Get<T>() => (TryParseDelegate<T>)Parsers.Value[typeof(T)];
        public static Delegate Get(Type type) => Parsers.Value[type];

        public static bool TryParseDate(string s, out DateTime date)
        {
            date = default;

            string[] formats = { 
                // Basic formats
                "yyyyMMddTHHmmsszzz",
                "yyyyMMddTHHmmsszz",
                "yyyyMMddTHHmmssZ",
                // Extended formats
                "yyyy-MM-ddTHH:mm:sszzz",
                "yyyy-MM-ddTHH:mm:sszz",
                "yyyy-MM-ddTHH:mm:ssZ",
                // All of the above with reduced accuracy
                "yyyyMMddTHHmmzzz",
                "yyyyMMddTHHmmzz",
                "yyyyMMddTHHmmZ",
                "yyyy-MM-ddTHH:mmzzz",
                "yyyy-MM-ddTHH:mmzz",
                "yyyy-MM-ddTHH:mmZ",
                // Accuracy reduced to hours
                "yyyyMMddTHHzzz",
                "yyyyMMddTHHzz",
                "yyyyMMddTHHZ",
                "yyyy-MM-ddTHHzzz",
                "yyyy-MM-ddTHHzz",
                "yyyy-MM-ddTHHZ"
                };

            if (DateTime.TryParse(s, out date))
                return true;

            return DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}
