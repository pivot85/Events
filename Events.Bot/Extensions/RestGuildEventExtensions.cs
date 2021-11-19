using Discord.Rest;
using System.Linq;
using System.Threading.Tasks;

namespace Events.Bot.Extensions
{
    public static class RestGuildEventExtensions
    {
        public static async Task<string> GetUrlAsync(this RestGuildEvent guildEvent) => $"https://discord.com/events/{guildEvent.Guild.Id}/{guildEvent.Id}";
    }
}
