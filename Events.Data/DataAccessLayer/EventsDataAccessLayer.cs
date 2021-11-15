namespace Events.Data.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Events.Data.Context;
    using Events.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The Data Access Layer for the Events Table.
    /// </summary>
    public class EventsDataAccessLayer : IEventsDataAccessLayer
    {
        private readonly IDbContextFactory<EventsDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsDataAccessLayer"/> class.
        /// </summary>
        /// <param name="contextFactory">The <see cref="IDbContextFactory{TContext}"/> to be injected.</param>
        public EventsDataAccessLayer(IDbContextFactory<EventsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Gets all events from all guilds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetAll()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .ToListAsync();
        }

        /// <summary>
        /// Get a single event by Id.
        /// </summary>
        /// <param name="eventId">The Id of the event that is requested.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<Event> GetByGuid(Guid eventId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.Id == eventId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all events in a guild.
        /// </summary>
        /// <param name="guild">The guild in which the events are being ran.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetAllByGuild(ulong guild)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.Guild == guild)
                .ToListAsync();
        }

        /// <summary>
        /// Get all events by their completion value.
        /// </summary>
        /// <param name="guild">The guild in which the events are bring run.</param>
        /// <param name="completionStatus">A bool representing the status.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetByCompletion(ulong guild, bool completionStatus)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.Guild == guild && x.IsCompleted == completionStatus)
                .ToListAsync();
        }

        /// <summary>
        /// Get an event by its name and guild.
        /// </summary>
        /// <param name="guild">The guild that the event is run in.</param>
        /// <param name="title">The title of the Event.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<Event> GetByTitle(ulong guild, string title)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.Guild == guild && x.Title == title)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Create a new event.
        /// </summary>
        /// <param name="event">The event to create.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Create(Event @event)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Add(@event);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the organiser of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="organiser">The new Id of the organiser.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateOrganiser(Guid eventId, ulong organiser)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Organiser = organiser;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the title of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="title">The title of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateTitle(Guid eventId, string title)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Title = title;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the start time of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventStart">The start time of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateStart(Guid eventId, DateTime eventStart)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Start = eventStart;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the duration of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventDuration">The duration of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateDuration(Guid eventId, TimeSpan eventDuration)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Duration = eventDuration;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Category Id of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="categoryId">The Id of the category of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateCategory(Guid eventId, ulong categoryId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.Category = categoryId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the Text Discussion Channel.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="textChannelId">The Id of the Text Channel for the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateTextChannel(Guid eventId, ulong textChannelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.TextChannel = textChannelId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the event Voice Channel.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="voiceChannelId">The Id of the voice channel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateVoiceChannel(Guid eventId, ulong voiceChannelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.VoiceChannel = voiceChannelId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the event control panel channel.
        /// </summary>
        /// <param name="eventId">The Id of the event which the control panel belongs.</param>
        /// <param name="controlPanelId">The Id of the channel for the control panel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateControlPanel(Guid eventId, ulong controlPanelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.ControlChannel = controlPanelId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the Event Steward Role.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="stewardRoleId">The Id of the steward Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateStewardRole(Guid eventId, ulong stewardRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.StewardRole = stewardRoleId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the speaker Role.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="speakerRoleId">The Id of the Speaker Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateSpeakerRole(Guid eventId, ulong speakerRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.SpeakerRole = speakerRoleId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update Id of the Attendee Role.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="attendeeRoleId">The Id of the attendee Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAttendeeRole(Guid eventId, ulong attendeeRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.AttendeeRole = attendeeRoleId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the Cosmetic Role of an event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="cosmeticRoleId">The Id of the cosmetic Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateCosmeticRole(Guid eventId, ulong cosmeticRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.CosmeticRole = cosmeticRoleId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the completion status of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventComplete">The status of if the event is completed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateCompletionStatus(Guid eventId, bool eventComplete)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetByGuid(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.IsCompleted = eventComplete;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an event.
        /// </summary>
        /// <param name="eventId">The id of the event to be deleted.</param>
        /// <returns>Nothing... Poof... its all gone! I Promise...</returns>
        public async Task Delete(Guid eventId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToDelete = await GetByGuid(eventId);
            if (eventToDelete is null)
            {
                return;
            }

            context.Remove(eventId);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Determines whether or not an event with specified short name exists.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="shortName">The short name to check.</param>
        /// <returns>A bool indicating whether or not the short name exists.</returns>
        public bool ShortNameExists(ulong guildId, string shortName)
        {
            using var context = _contextFactory.CreateDbContext();

            var @event = context.Events
                .Where(x => x.IsCompleted == false && x.Guild == guildId)?
                .FirstOrDefault(x => x.ShortName.ToLower() == shortName.ToLower());

            if (@event == null)
            {
                return false;
            }

            return true;
        }
    }
}