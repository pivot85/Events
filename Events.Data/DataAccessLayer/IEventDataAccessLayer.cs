using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Events.Data.Models;

namespace Events.Data.DataAccessLayer
{
    public interface IEventDataAccessLayer
    {
        // Read
        public Task<IEnumerable<Event>> GetAllEvents();
        public Task<Event> GetEventByGuidAsync(Guid eventId);
        public Task<IEnumerable<Event>> GetAllEventsByGuildAsync(ulong guild);
        public Task<IEnumerable<Event>> GetEventsByCompletionAsync(ulong guildId, bool completionStatus);
        public Task<Event> GetEventByTitle(ulong guildId, string title);
        
        // Create
        public Task CreateNewEvent(Guid eventId, ulong guildId, ulong organiserId, string eventTitle,
            DateTime eventStart,
            TimeSpan eventDuration, ulong categoryId, ulong textChannelId, ulong voiceChannelId, ulong controlPanelId,
            ulong stewardRankId, ulong speakerRankId, ulong attendeeRankId, bool eventComplete, ulong cosmeticRankId);

        // Update

        public Task UpdateEventOrganiser(Guid eventId, ulong organiserId);
        public Task UpdateEventTitle(Guid eventId, string eventTitle);
        public Task UpdateEventStart(Guid eventId, DateTime eventStart);
        public Task UpdateEventDuration(Guid eventId, TimeSpan eventDuration);
        public Task UpdateCategoryId(Guid eventId, ulong categoryId);
        public Task UpdateTextChannelId(Guid eventId, ulong textChannelId);
        public Task UpdateVoiceChannelId(Guid eventId, ulong voiceChannelId);
        public Task UpdateControlPanelId(Guid eventId, ulong controlPanelId);
        public Task UpdateStewardRankId(Guid eventId, ulong stewardRankId);
        public Task UpdateSpeakerRankId(Guid eventId, ulong speakerRankId);
        public Task UpdateAttendeeRankId(Guid eventId, ulong attendeeRankId);
        public Task UpdateCosmeticRankId(Guid eventId, ulong cosmeticRankId);
        public Task UpdateEventCompletionStatus(Guid eventId, bool eventComplete);

        // Delete
        public Task DeleteEvent(Guid eventId);

    }
}