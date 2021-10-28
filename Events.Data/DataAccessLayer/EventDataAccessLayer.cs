using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events.Data.Context;
using Events.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Data.DataAccessLayer
{
    public class EventDataAccessLayer : IEventDataAccessLayer
    {
        private readonly EventDbContext _dbContext;

        public EventDataAccessLayer(EventDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Event>> GetAllEvents()
        {
            return await _dbContext.Events
                .ToListAsync();
        }

        public async Task<Event> GetEventByGuidAsync(Guid eventId)
        {
            return await _dbContext.Events
                .Where(x => x.Id == eventId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Event>> GetAllEventsByGuildAsync(ulong guildId)
        {
            return await _dbContext.Events
                .Where(x => x.GuildId == guildId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByCompletionAsync(ulong guildId, bool completionStatus)
        {
            return await _dbContext.Events
                .Where(x => x.GuildId == guildId && x.IsCompleted == completionStatus)
                .ToListAsync();
        }

        public async Task<Event> GetEventByTitle(ulong guildId, string title)
        {
            return await _dbContext.Events
                .Where(x => x.GuildId == guildId && x.EventTitle == title)
                .FirstOrDefaultAsync();
        }

        public async Task CreateNewEvent(Guid eventId, ulong guildId, ulong organiserId, string eventTitle, DateTime eventStart,
            TimeSpan eventDuration, ulong categoryId, ulong textChannelId, ulong voiceChannelId, ulong controlPanelId,
            ulong stewardRankId, ulong speakerRankId, ulong attendeeRankId, bool eventComplete, ulong cosmeticRankId)
        {
            _dbContext.Add(new Event
            {
                Id = eventId,
                GuildId = guildId,
                OrganiserId = organiserId,
                EventTitle = eventTitle,
                StartTime = eventStart,
                Duration = eventDuration,
                Category = categoryId,
                TextChannel = textChannelId,
                VoiceChannel = voiceChannelId,
                ControlChannel = controlPanelId,
                StewardRank = stewardRankId,
                SpeakerRank = speakerRankId,
                AttendeeRank = attendeeRankId,
                CosmeticRank = cosmeticRankId,
                IsCompleted = eventComplete

            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEventOrganiser(Guid eventId, ulong organiserId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.OrganiserId = organiserId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEventTitle(Guid eventId, string eventTitle)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.EventTitle = eventTitle;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEventStart(Guid eventId, DateTime eventStart)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.StartTime = eventStart;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEventDuration(Guid eventId, TimeSpan eventDuration)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Duration = eventDuration;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCategoryId(Guid eventId, ulong categoryId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Category = categoryId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTextChannelId(Guid eventId, ulong textChannelId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.TextChannel = textChannelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateVoiceChannelId(Guid eventId, ulong voiceChannelId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.VoiceChannel = voiceChannelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateControlPanelId(Guid eventId, ulong controlPanelId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.ControlChannel = controlPanelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateStewardRankId(Guid eventId, ulong stewardRankId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.StewardRank = stewardRankId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSpeakerRankId(Guid eventId, ulong speakerRankId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.SpeakerRank = speakerRankId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAttendeeRankId(Guid eventId, ulong attendeeRankId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.AttendeeRank = attendeeRankId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCosmeticRankId(Guid eventId, ulong cosmeticRankId)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.CosmeticRank = cosmeticRankId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEventCompletionStatus(Guid eventId, bool eventComplete)
        {
            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.IsCompleted = eventComplete;
            await _dbContext.SaveChangesAsync();
        }


        public async Task DeleteEvent(Guid eventId)
        {
            var eventToDelete = await GetEventByGuidAsync(eventId);
            if (eventToDelete is null)
            {
                return;
            }

            _dbContext.Remove(eventId);
            await _dbContext.SaveChangesAsync();
        }
    }
}