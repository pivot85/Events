using Discord.Rest;
using System.Linq;
using System.Threading.Tasks;

namespace Events.Bot.Extensions
{
    public static class RestGuildEventExtensions
    {
        public static async Task<string> GetUrlAsync(this RestGuildEvent guildEvent, ulong channel)
        {
            string inviteCode = string.Empty;
            var invites = await guildEvent.Guild.GetInvitesAsync();
            var currentUser = await guildEvent.Guild.GetCurrentUserAsync();

            if (invites.Any(x => x.Inviter.Id == currentUser.Id && x.MaxAge == null))
                inviteCode = invites.First(x => x.Inviter.Id == currentUser.Id && x.MaxAge == null).Code;
            else
                inviteCode = (await (await guildEvent.Guild.GetVoiceChannelAsync(channel)).CreateInviteAsync(null)).Code;

            return $"https://discord.gg/{inviteCode}?event={guildEvent.Id}";
        }
    }
}
