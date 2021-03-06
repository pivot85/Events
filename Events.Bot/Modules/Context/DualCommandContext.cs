using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot
{
    public class DualCommandContext : ICommandContext
    {
        public ISocketMessageChannel Channel { get; }
        public DiscordSocketClient Client { get; }
        public SocketUser User { get; }
        public SocketGuild Guild { get; }
        public SocketUserMessage Message { get; }
        public SocketInteraction Interaction { get; set; }

        public bool IsInteraction
            => Interaction != null;

        public DualCommandContext(DiscordSocketClient client, SocketInteraction interaction)
        {
            this.Client = client;
            this.User = interaction.User;
            this.Guild = (interaction.Channel as SocketGuildChannel)?.Guild;
            this.Channel = interaction.Channel;
            this.Interaction = interaction;
            this.Message = (interaction as SocketMessageComponent)?.Message ?? ((interaction as SocketMessageCommand)?.Data.Message as SocketUserMessage);
        }

        public DualCommandContext(DiscordSocketClient client, SocketUserMessage message)
        {
            this.Client = client;
            this.User = message.Author;
            this.Guild = (message.Channel as SocketGuildChannel)?.Guild;
            this.Channel = message.Channel;
            this.Message = message;
        }

        public async Task<IUserMessage> ReplyAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, RequestOptions options = null, MessageComponent component = null, Embed embed = null)
        {
            if (this.Interaction == null)
            {
                return await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, component);
            }
            else
            {
                await Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed);
            }

            return null;
        }

        // ICommandContext
        IDiscordClient ICommandContext.Client
            => Client;
        IGuild ICommandContext.Guild
            => Guild;
        IMessageChannel ICommandContext.Channel
            => Channel;
        IUser ICommandContext.User
            => User;
        IUserMessage ICommandContext.Message
            => Message;
    }
}
