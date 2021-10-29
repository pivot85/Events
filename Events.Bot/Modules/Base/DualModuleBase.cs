using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
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
    }
}
