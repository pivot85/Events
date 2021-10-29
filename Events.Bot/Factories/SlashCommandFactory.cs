using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Factories
{
    public class SlashCommandFactory : ApplicationCommandFactory
    {
        [GuildSpecificCommand(902654591710138408)]
        public override IEnumerable<ApplicationCommandProperties> BuildCommands()
        {
            var pingCommand = new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Ping for a pong!");

            var pongCommand = new SlashCommandBuilder()
                .WithName("pong")
                .WithDescription("poggers");

            var newEventCommand = new SlashCommandBuilder()
                .WithName("new")
                .WithDescription("Create a new event interactively!");

            return new ApplicationCommandProperties[] { pingCommand.Build(), pongCommand.Build(), newEventCommand.Build() };
        }
    }
}
