using Discord;
using Discord.Addons.Hosting;
using Discord.Rest;
using Discord.WebSocket;
using Events.Data.DataAccessLayer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Client.GuildScheduledEventUserAdd += Client_GuildScheduledEventUserAdd;
            Client.GuildScheduledEventUserRemove += Client_GuildScheduledEventUserRemove;
            return Task.CompletedTask;
        }

        private async Task Client_GuildScheduledEventUserRemove(Cacheable<SocketUser, RestUser, IUser, ulong> cachedUser, SocketGuildEvent guildEvent)
        {
            var storedEvent = await _eventsDataAccessLayer.GetById(guildEvent.Id);
            if (storedEvent == null)
                return;

            var attendeeRole = guildEvent.Guild.GetRole(storedEvent.AttendeeRole);
            if (attendeeRole == null)
                return;

            var user = await cachedUser.GetOrDownloadAsync() as SocketGuildUser;
            if (!user.Roles.Contains(attendeeRole))
                return;

            await user.RemoveRoleAsync(attendeeRole);
        }

        private async Task Client_GuildScheduledEventUserAdd(Cacheable<SocketUser, RestUser, IUser, ulong> cachedUser, SocketGuildEvent guildEvent)
        {
            var storedEvent = await _eventsDataAccessLayer.GetById(guildEvent.Id);
            if (storedEvent == null)
                return;

            var attendeeRole = guildEvent.Guild.GetRole(storedEvent.AttendeeRole);
            if (attendeeRole == null)
                return;

            var user = await cachedUser.GetOrDownloadAsync() as SocketGuildUser;
            if (user.Roles.Contains(attendeeRole))
                return;

            await user.AddRoleAsync(attendeeRole);
        }
    }
}
