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
            var newEventCommand = new SlashCommandBuilder()
                .WithName("new")
                .WithDescription("Create a new event interactively!");

            var clearCommand = new SlashCommandBuilder()
                .WithName("clear")
                .WithDescription("Clean-up all event related entities (temporary).");

            return new ApplicationCommandProperties[] { newEventCommand.Build(), clearCommand.Build() };
        }
    }
}
