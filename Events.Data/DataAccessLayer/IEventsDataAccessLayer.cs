namespace Events.Data.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Events.Data.Models;

    /// <summary>
    /// Implementation Contract of <see cref="EventsDataAccessLayer"/>.
    /// </summary>
    public interface IEventsDataAccessLayer
    {
        // Read
        public Task<IEnumerable<Event>> GetAllEvents();

        public Task<Event> GetEventByGuidAsync(Guid eventId);

        public Task<IEnumerable<Event>> GetAllEventsByGuildAsync(ulong guild);

        public Task<IEnumerable<Event>> GetEventsByCompletionAsync(ulong guildId, bool completionStatus);

        public Task<Event> GetEventByTitle(ulong guildId, string title);

        // Create
        public Task CreateNewEvent(Guid eventId, ulong guildId, ulong organiser, string eventTitle,
            DateTime eventStart,
            TimeSpan eventDuration, ulong categoryId, ulong textChannelId, ulong voiceChannelId, ulong controlPanelId,
            ulong stewardRoleId, ulong speakerRoleId, ulong attendeeRoleId, ulong cosmeticRoleId, bool eventComplete);

        // Update
        public Task UpdateEventOrganiser(Guid eventId, ulong organiser);

        public Task UpdateTitle(Guid eventId, string eventTitle);

        public Task UpdateStart(Guid eventId, DateTime eventStart);

        public Task UpdateDuration(Guid eventId, TimeSpan eventDuration);

        public Task UpdateCategoryId(Guid eventId, ulong categoryId);

        public Task UpdateTextChannelId(Guid eventId, ulong textChannelId);

        public Task UpdateVoiceChannelId(Guid eventId, ulong voiceChannelId);

        public Task UpdateControlPanelId(Guid eventId, ulong controlPanelId);

        public Task UpdateStewardRoleId(Guid eventId, ulong stewardRoleId);

        public Task UpdateSpeakerRoleId(Guid eventId, ulong speakerRoleId);

        public Task UpdateAttendeeRoleId(Guid eventId, ulong attendeeRoleId);

        public Task UpdateCosmeticRoleId(Guid eventId, ulong cosmeticRoleId);

        public Task UpdateEventCompletionStatus(Guid eventId, bool eventComplete);

        // Delete
        public Task DeleteEvent(Guid eventId);
    }
}