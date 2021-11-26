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

        public Task<Event> GetById(ulong eventId);

        public Task<IEnumerable<Event>> GetAllByGuild(ulong guild);

        public Task<IEnumerable<Event>> GetByCompletion(ulong guildId, bool completionStatus);

        public Task<Event> GetByTitle(ulong guildId, string title);

        public bool ShortNameExists(ulong guildId, string shortName);

        // Create
        public Task Create(Event @event);

        // Update
        public Task UpdateOrganiser(ulong eventId, ulong organiser);

        public Task UpdateTitle(ulong eventId, string eventTitle);

        public Task UpdateStart(ulong eventId, DateTime eventStart);

        public Task UpdateDuration(ulong eventId, TimeSpan eventDuration);

        public Task UpdateCategory(ulong eventId, ulong categoryId);

        public Task UpdateTextChannel(ulong eventId, ulong textChannelId);

        public Task UpdateVoiceChannel(ulong eventId, ulong voiceChannelId);

        public Task UpdateControlPanel(ulong eventId, ulong controlPanelId);

        public Task UpdateStewardRole(ulong eventId, ulong stewardRoleId);

        public Task UpdateSpeakerRole(ulong eventId, ulong speakerRoleId);

        public Task UpdateAttendeeRole(ulong eventId, ulong attendeeRoleId);

        public Task UpdateCosmeticRole(ulong eventId, ulong cosmeticRoleId);

        public Task UpdateCompletionStatus(ulong eventId, bool eventComplete);

        // Delete
        public Task Delete(ulong eventId);
    }
}