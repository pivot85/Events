using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Events.Data.DataAccessLayer;
using Interactivity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot
{
    /// <summary>
    ///     Represents a module base for interaction based commands and message based commands.
    /// </summary>
    public class DualModuleBase
    {
        private bool _hasDefferd = false;

        /// <summary>
        ///     Gets the context for the command.
        /// </summary>
        public DualCommandContext Context { get; private set; }
        public InteractivityService Interactivity { get; set; }
        public readonly EventsDataAccessLayer EventsDataAccessLayer;
        public readonly PermittedRolesDataAccessLayer PermittedRoleDataAccessLayer;
        public readonly IConfiguration Configuration;

        public DualModuleBase(EventsDataAccessLayer eventsDataAccessLayer, PermittedRolesDataAccessLayer permittedRoleDataAccessLayer, InteractivityService interactivityService, IConfiguration configuration)
        {
            EventsDataAccessLayer = eventsDataAccessLayer;
            PermittedRoleDataAccessLayer = permittedRoleDataAccessLayer;
            Interactivity = interactivityService;
            Configuration = configuration;
        }

        public void SetContext(ICommandContext context)
        {
            this.Context = (DualCommandContext)context;
        }

        public virtual async Task BeforeExecuteAsync() { }

        /// <summary>
        ///     Replies to a command
        /// </summary>
        /// <remarks>
        ///     If the command is an interaction this method will use the interactions respond/followup methods. If
        ///     its a message based command it will send the message to the channel of the executing command.
        /// </remarks>
        public async Task<IUserMessage> ReplyAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, RequestOptions options = null, MessageComponent component = null, Embed embed = null)
        {
            if (this.Context.Interaction == null)
            {
                return await Context.Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, component);
            }
            else
            {
                if (!_hasDefferd)
                    await Context.Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed);
                else
                    return await Context.Interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed);
            }

            return null;
        }

        /// <summary>
        ///     Defers the interaction.
        /// </summary>
        /// <remarks>
        ///     If the command being executed is not an interaction this method will just return.
        /// </remarks>
        /// <param name="ephemeral">whether or not to defer ephemerally</param>
        public Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {
            if (Context.IsInteraction)
            {
                this._hasDefferd = true;
                return Context.Interaction.DeferAsync(ephemeral, options);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Follows up to an interaction.
        /// </summary>
        /// <remarks>
        ///     If this command is not interaction based, this method will send a message to the channel of the executing command.
        /// </remarks>
        public Task<IUserMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, RequestOptions options = null, MessageComponent component = null, Embed embed = null)
        {
            if (Context.IsInteraction)
                return Context.Interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed).ContinueWith(x => (IUserMessage)x);
            else
                return ReplyAsync(text, embeds, isTTS, ephemeral, allowedMentions, messageReference, options, component, embed);
        }

        public async Task<object[]> CreateInteractionArgsAsync(CommandInfo info, DualCommandService service, object[] args)
        {
            var returnParams = new List<object>();
            if (!Context.IsInteraction)
                return args;

            for (int i = 0; i != info.Parameters.Count; i++)
            {
                var param = info.Parameters[i];

                if (Context.Interaction is SocketSlashCommand slash)
                {
                    try
                    {
                        var opts = slash.Data.Options;

                        if (opts == null)
                        {
                            returnParams.Add(Type.Missing);
                            continue;
                        }

                        while (opts.Count == 1 && opts.First().Type == ApplicationCommandOptionType.SubCommand)
                        {
                            opts = opts.First().Options;
                        }

                        var slashParam = opts?.FirstOrDefault(x => x.Name == param.Name);

                        if (slashParam == null)
                        {
                            returnParams.Add(Type.Missing);
                            continue;
                        }

                        var tp = slashParam.Value.GetType();

                        object value = null;

                        var reader = service.GetTypeReader(param.Type);

                        if (tp.IsAssignableTo(param.Type))
                            value = slashParam.Value;
                        else if (reader != null && slashParam.Type == ApplicationCommandOptionType.String || 
                                                   slashParam.Type == ApplicationCommandOptionType.Number || 
                                                   slashParam.Type == ApplicationCommandOptionType.Boolean ||
                                                   slashParam.Type == ApplicationCommandOptionType.Integer)
                        {
                            var result = await reader.ReadAsync(Context, slashParam.Value?.ToString(), null);

                            if (result.IsSuccess)
                                value = result.Values.FirstOrDefault().Value;
                            else
                            {
                                await param.HandleErrorAsync(Context, slashParam.Value).ConfigureAwait(false);
                                return null;
                            }
                        }
                        else if (InternalConverters.ContainsKey((tp, param.Type)))
                        {
                            try
                            {
                                value = InternalConverters[(tp, param.Type)].Invoke(slashParam.Value);
                            }
                            catch (Exception x)
                            {
                                await param.HandleErrorAsync(Context, slashParam.Value).ConfigureAwait(false);
                                return null;
                            }
                        }
                        else if (slashParam.Type == ApplicationCommandOptionType.Integer && param.Type.IsEnum)
                            value = Enum.ToObject(param.Type, slashParam.Value);
                        else
                        {
                            await param.HandleErrorAsync(Context, slashParam.Value).ConfigureAwait(false);
                            return null;
                        }
                        returnParams.Add(value);
                    }
                    catch
                    {
                        await param.HandleErrorAsync(Context, null).ConfigureAwait(false);
                        return null;
                    }
                }
            }

            return returnParams.ToArray();
        }

        private Dictionary<(Type from, Type to), Func<object, object>> InternalConverters = new Dictionary<(Type from, Type to), Func<object, object>>()
        {
            {(typeof(long), typeof(int)), (v) => { return Convert.ToInt32(v); } },
        };

        public async Task<bool> UserIsPermitted()
        {
            var user = Context.User as SocketGuildUser;

            if (user.GuildPermissions.Administrator)
                return true;

            var permittedRoles = await PermittedRoleDataAccessLayer.GetAllByGuild(Context.Guild.Id);
            if (permittedRoles.Count() == 0)
                return false;

            if (user.Roles.Select(x => x.Id).Intersect(permittedRoles.Select(x => x.Id)).Any())
                return true;

            return false;
        }

        public async Task<InteractivityResult<SocketMessage>> NextMessageAsync()
        {
            return await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id);
        }

        public async Task<T> Ask<T>(
            int min = 0,
            int max = 0,
            string minMaxError = null,
            string cancelMessage = "Cancelled.",
            bool shortNameCheck = false,
            string timedOutMessage = "You didn't respond in time, please run the command again.",
            string parseFailedMessage = "Please provide a response containing the correct type.")
        {
            switch (typeof(T))
            {
                case var cls when cls == typeof(string):
                    string @string = string.Empty;
                    while (true)
                    {
                        var response = await NextMessageAsync();
                        if (response.IsTimeouted || response.Value == null)
                        {
                            await Context.Channel.SendMessageAsync(timedOutMessage);
                            return default(T);
                        }

                        string content = response.Value.Content;

                        if (content.ToLower() == "cancel")
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        if (max > 0 && content.Length > max)
                        {
                            await Context.Channel.SendMessageAsync(minMaxError);
                            continue;
                        }

                        if (shortNameCheck == true && Context.Guild.CategoryChannels.Any(x => x.Name.ToLower().Contains(content.ToLower())))
                        {
                            await Context.Channel.SendMessageAsync($"That short name is already in use, please provide a different one.");
                            continue;
                        }

                        @string = content;
                        break;
                    }
                    return (T)Convert.ChangeType(@string, typeof(T));
                case var cls when cls == typeof(DateTime):
                    var dateTime = new DateTime();
                    while (true)
                    {
                        var response = await NextMessageAsync();
                        if (response.IsTimeouted || response.Value == null)
                        {
                            await Context.Channel.SendMessageAsync(timedOutMessage);
                            return default(T);
                        }

                        string content = response.Value.Content;

                        if (content.ToLower() == "cancel")
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        if (!DateTime.TryParse(content, out dateTime))
                        {
                            await Context.Channel.SendMessageAsync(parseFailedMessage);
                            continue;
                        }
                        break;
                    }
                    return (T)Convert.ChangeType(dateTime, typeof(T));
                case var cls when cls == typeof(TimeSpan):
                    var timeSpan = new TimeSpan();
                    while (true)
                    {
                        var response = await NextMessageAsync();
                        if (response.IsTimeouted || response.Value == null)
                        {
                            await Context.Channel.SendMessageAsync(timedOutMessage);
                            return default(T);
                        }

                        string content = response.Value.Content;

                        if (content.ToLower() == "cancel")
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        if (!TimeSpan.TryParse(content, out timeSpan))
                        {
                            await Context.Channel.SendMessageAsync(parseFailedMessage);
                            continue;
                        }

                        if (timeSpan < TimeSpan.FromMinutes(min) || timeSpan >= TimeSpan.FromHours(max))
                        {
                            await Context.Channel.SendMessageAsync(minMaxError);
                            continue;
                        }

                        break;
                    }
                    return (T)Convert.ChangeType(timeSpan, typeof(T));
                case var cls when cls == typeof(List<RestGuildUser>):
                    var users = new List<RestGuildUser>();
                    while (true)
                    {
                        var response = await NextMessageAsync();
                        if (response.IsTimeouted || response.Value == null)
                        {
                            await Context.Channel.SendMessageAsync(timedOutMessage);
                            return default(T);
                        }

                        string content = response.Value.Content;

                        if (content.ToLower() == "cancel")
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        if (content.ToLower() == "skip")
                            break;

                        foreach (var mention in content.Split(" "))
                        {
                            if (!MentionUtils.TryParseUser(mention, out ulong userId))
                                continue;

                            var user = await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, userId);
                            if (user == null || users.Contains(user))
                                continue;

                            users.Add(user);
                        }

                        break;
                    }
                    return (T)Convert.ChangeType(users, typeof(T));
                case var cls when cls == typeof(SocketRole):
                    var socketRole = Context.Guild.EveryoneRole;
                    while (true)
                    {
                        var response = await NextMessageAsync();
                        if (response.IsTimeouted || response.Value == null)
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        string content = response.Value.Content;

                        if (content.ToLower() == "cancel")
                        {
                            await Context.Channel.SendMessageAsync(cancelMessage);
                            return default(T);
                        }

                        if (content.ToLower() == "skip")
                            break;

                        if (!MentionUtils.TryParseRole(content, out ulong roleId))
                        {
                            await Context.Channel.SendMessageAsync(parseFailedMessage);
                            continue;
                        }

                        var role = Context.Guild.GetRole(roleId);
                        if (role == null)
                        {
                            await Context.Channel.SendMessageAsync("That role does not exist, please provide a valid role.");
                            continue;
                        }

                        socketRole = role;
                        break;
                    }
                    return (T)Convert.ChangeType(socketRole, typeof(T));
            }

            throw new NotImplementedException("This type could not be requested.");
        }
    }
}
