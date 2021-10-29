﻿using Discord.Commands;
using Events.Data.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class PingModule : DualModuleBase
    {
        public PingModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRoleDataAccessLayer permittedRoleDataAccessLayer)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer)
        {
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync($"Pong! {Context.Client.Latency}");
        }
    }
}
