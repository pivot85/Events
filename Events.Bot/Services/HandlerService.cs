using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot
{
    public abstract class DiscordHandler
    {
        public virtual Task InitializeAsync(DiscordSocketClient client, IServiceProvider provider)
        {
            return Task.CompletedTask;
        }

        public virtual void Initialize(DiscordSocketClient client, IServiceProvider provider)
        {
        }
    }
}

namespace Events.Bot
{
    /// <summary>
    ///     Represents a service for managing discord handlers.
    /// </summary>
    public class HandlerService
    {
        private readonly DiscordSocketClient _client;
        private IServiceProvider _provider => _coll.BuildServiceProvider();
        private readonly ServiceCollection _coll;
        private readonly Logger _log;
        private static readonly Dictionary<DiscordHandler, object> _handlers = new Dictionary<DiscordHandler, object>();

        /// <summary>
        ///     Retrivies an instance of a handler with the given type.
        /// </summary>
        /// <typeparam name="T">The type of handler to get.</typeparam>
        /// <returns>A handlers instance if found; otherwise <see langword="null"/>.</returns>
        public static T GetHandlerInstance<T>()
            where T : DiscordHandler => _handlers.FirstOrDefault(x => x.Key.GetType() == typeof(T)).Value as T;

        private bool _hasInit = false;
        private object _lock = new object();

        /// <summary>
        ///     Creates a new instance of the handler service.
        /// </summary>
        /// <param name="client">The discord client to proxy to the handlers.</param>
        /// <param name="provider">A provider containing all the registered services.</param>
        public HandlerService(DiscordSocketClient client, ServiceCollection provider)
        {
            this._client = client;
            this._coll = provider;
            this._client.Ready += Client_Ready;
            _log = Logger.GetLogger<HandlerService>();

            List<Type> typs = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAssignableTo(typeof(DiscordHandler)) && type != typeof(DiscordHandler))
                    {
                        typs.Add(type);
                    }
                }
            }

            foreach (var handler in typs)
            {
                var inst = Activator.CreateInstance(handler);
                _handlers.Add(inst as DiscordHandler, inst);
            }

            _log.Log($"Created {_handlers.Count} handlers");
        }

        private Task Client_Ready()
        {
            lock (_lock)
            {
                if (!_hasInit)
                {
                    _hasInit = true;
                }
                else return Task.CompletedTask;
            }

            _ = Task.Run(() =>
            {
                var work = new List<Func<Task>>();

                foreach (var item in _handlers)
                {
                    work.Add(async () =>
                    {
                        try
                        {
                            await item.Key.InitializeAsync(this._client, _provider);
                            item.Key.Initialize(this._client, _provider);
                        }
                        catch (Exception x)
                        {
                            _log.Error($"Exception occured while initializing {item.Key.GetType().Name}: ", exception: x);
                        }
                    });
                }

                Task.WaitAll(work.Select(x => x()).ToArray());

                _log.Info($"Initialized <Green>{_handlers.Count}</Green> handlers!");
            });

            return Task.CompletedTask;
        }
    }
}
