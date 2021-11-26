using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Events.Data.DataAccessLayer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Bot.Handlers
{
    public class ScheduledEventHandler : DiscordClientService
    {
        private readonly EventsDataAccessLayer _eventsDataAccessLayer;

        public ScheduledEventHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, EventsDataAccessLayer eventsDataAccessLayer)
            : base(client, logger)
        {
            _eventsDataAccessLayer = eventsDataAccessLayer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
