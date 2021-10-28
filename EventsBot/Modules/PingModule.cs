using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class PingModule : DualModuleBase
    {
        [Command("ping")]
        public async Task PingCommand()
        {
            await ReplyAsync($"Pong! {Context.Client.Latency}");
        }
    }
}
