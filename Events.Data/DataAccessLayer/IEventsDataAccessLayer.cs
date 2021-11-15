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
        public Task<IEnumerable<Event>> GetAll();

        public Task<Event> GetByGuid(Guid eventId);

        public Task<IEnumerable<Event>> GetAllByGuild(ulong guild);

        public Task<IEnumerable<Event>> GetByCompletion(ulong guildId, bool completionStatus);

        public Task<Event> GetByTitle(ulong guildId, string title);

        public bool ShortNameExists(ulong guildId, string shortName);

        // Create
        public Task Create(Event @event);

        // Update
        public Task UpdateOrganiser(Guid eventId, ulong organiser);

        public Task UpdateTitle(Guid eventId, string eventTitle);

        public Task UpdateStart(Guid eventId, DateTime eventStart);

        public Task UpdateDuration(Guid eventId, TimeSpan eventDuration);

        public Task UpdateCategory(Guid eventId, ulong categoryId);

        public Task UpdateTextChannel(Guid eventId, ulong textChannelId);

        public Task UpdateVoiceChannel(Guid eventId, ulong voiceChannelId);

        public Task UpdateControlPanel(Guid eventId, ulong controlPanelId);

        public Task UpdateStewardRole(Guid eventId, ulong stewardRoleId);

        public Task UpdateSpeakerRole(Guid eventId, ulong speakerRoleId);

        public Task UpdateAttendeeRole(Guid eventId, ulong attendeeRoleId);

        public Task UpdateCosmeticRole(Guid eventId, ulong cosmeticRoleId);

        public Task UpdateCompletionStatus(Guid eventId, bool eventComplete);

        // Delete
        public Task Delete(Guid eventId);
    }
}