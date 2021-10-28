using Discord;
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
    ///     Represents a base class for factories to use when creating commands.
    /// </summary>
    public abstract class ApplicationCommandFactory
    {
        /// <summary>
        ///     Gets a discord client.
        /// </summary>
        public DiscordSocketClient Client;

        /// <summary>
        ///     A method used to build commands.
        /// </summary>
        /// <returns>A collection containing properties to create commands.</returns>
        public abstract IEnumerable<ApplicationCommandProperties> BuildCommands();

        /// <summary>
        ///     Fired when one command is registered.
        /// </summary>
        /// <param name="command">The command that was registered.</param>
        public virtual Task OnRegisterSingleAsync(RestApplicationCommand command) { return Task.CompletedTask; }

        /// <summary>
        ///     Fired when one command is registered.
        /// </summary>
        /// <param name="command">The command that was registered.</param>
        public virtual void OnRegisterSingle(RestApplicationCommand command) { }

        /// <summary>
        ///     Fired when all the commands are registered.
        /// </summary>
        /// <param name="commands">A collection of the commands that were registered.</param>
        public virtual Task OnRegisterAllAsync(IReadOnlyCollection<RestApplicationCommand> commands) { return Task.CompletedTask; }

        /// <summary>
        ///     Fired when all the commands are registered.
        /// </summary>
        /// <param name="commands">A collection of the commands that were registered.</param>
        public virtual void OnRegisterAll(IReadOnlyCollection<RestApplicationCommand> commands) { }
    }

    public class GuildSpecificCommand : Attribute
    {
        public ulong GuildId { get; }

        public GuildSpecificCommand(ulong guildId)
        {
            this.GuildId = guildId;
        }
    }

    public class RequreReadyEvent : Attribute { }
}
