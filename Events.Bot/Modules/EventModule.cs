using Discord.Commands;
using Discord.WebSocket;
using Events.Data.DataAccessLayer;
using Interactivity;
using System.Linq;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class EventModule : DualModuleBase
    {
        public EventModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRolesDataAccessLayer permittedRoleDataAccessLayer, InteractivityService interactivityService)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer, interactivityService)
        {
        }

        [Command("newevent")]
        public async Task NewEventAsync()
        {
            var permittedRoles = await PermittedRoleDataAccessLayer.GetAllByGuild(Context.Guild.Id);
            if (!await UserIsPermitted())
            {
                await ReplyAsync("Not allowed!");
                return;
            }

            await ReplyAsync("Let's create a new event.");
        }
    }
}
