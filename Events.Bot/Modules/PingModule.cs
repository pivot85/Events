using Discord.Commands;
using Events.Data.DataAccessLayer;
using Interactivity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class PingModule : DualModuleBase
    {
        public PingModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRolesDataAccessLayer permittedRoleDataAccessLayer, InteractivityService interactivityService, IConfiguration configuration)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer, interactivityService, configuration)
        {
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync($"Pong! {Context.Client.Latency}");
        }
    }
}
