using Discord.Commands;
using Events.Data.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class EventModule : DualModuleBase
    {
        public EventModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRoleDataAccessLayer permittedRoleDataAccessLayer)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer)
        {
        }

        [Command("newevent")]
        public async Task NewEventAsync()
        {
            await ReplyAsync("This works!");
        }
    }
}
