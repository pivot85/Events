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
    public class EventDataAccessLayer : IEventDataAccessLayer
    {
        private readonly IDbContextFactory<EventsDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDataAccessLayer"/> class.
        /// </summary>
        /// <param name="contextFactory">The <see cref="IDbContextFactory{TContext}"/> to be injected.</param>
        public EventDataAccessLayer(IDbContextFactory<EventsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Gets all events from all guilds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetAllEvents()
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
        public async Task<Event> GetEventByGuidAsync(Guid eventId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.Id == eventId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all events in a guild.
        /// </summary>
        /// <param name="guildId">The guild in which the events are being ran.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetAllEventsByGuildAsync(ulong guildId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.GuildId == guildId)
                .ToListAsync();
        }

        /// <summary>
        /// Get all events by their completion value.
        /// </summary>
        /// <param name="guildId">The guild in which the events are bring run.</param>
        /// <param name="completionStatus">A bool representing the status.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<Event>> GetEventsByCompletionAsync(ulong guildId, bool completionStatus)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.GuildId == guildId && x.IsCompleted == completionStatus)
                .ToListAsync();
        }

        /// <summary>
        /// Get an event by its name and guild.
        /// </summary>
        /// <param name="guildId">The guild that the event is run in.</param>
        /// <param name="title">The title of the Event.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<Event> GetEventByTitle(ulong guildId, string title)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Events
                .Where(x => x.GuildId == guildId && x.EventTitle == title)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Create a new event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="guildId">The Id of the guild that the event is run.</param>
        /// <param name="organiserId">The Id of the organiser of the event.</param>
        /// <param name="eventTitle">The title of the event.</param>
        /// <param name="eventStart">The start-time of the event.</param>
        /// <param name="eventDuration">The duration of the event.</param>
        /// <param name="categoryId">The Id of the category of the event.</param>
        /// <param name="textChannelId">The Id of the generated discussion channel of the event.</param>
        /// <param name="voiceChannelId">The Id of the generated voice channel of the event. </param>
        /// <param name="controlPanelId">The Id of the Control Panel Channel of the event.</param>
        /// <param name="stewardRankId">The Id of the Steward (Event Moderator) Rank. </param>
        /// <param name="speakerRankId">The Id of the Speaker Rank.</param>
        /// <param name="attendeeRankId">The Id of the Attendee Rank.</param>
        /// <param name="cosmeticRankId">The Id of the Cosmetic Rank.</param>
        /// <param name="eventComplete">A bool representing the status.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateNewEvent(Guid eventId, ulong guildId, ulong organiserId, string eventTitle, DateTime eventStart,
            TimeSpan eventDuration, ulong categoryId, ulong textChannelId, ulong voiceChannelId, ulong controlPanelId,
            ulong stewardRankId, ulong speakerRankId, ulong attendeeRankId, ulong cosmeticRankId, bool eventComplete)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Add(new Event
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
                IsCompleted = eventComplete,
            });

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the organiser of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="organiserId">The new Id of the organiser.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventOrganiser(Guid eventId, ulong organiserId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.OrganiserId = organiserId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the title of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventTitle">The title of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventTitle(Guid eventId, string eventTitle)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.EventTitle = eventTitle;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the start time of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventStart">The start time of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventStart(Guid eventId, DateTime eventStart)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.StartTime = eventStart;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the duration of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventDuration">The duration of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventDuration(Guid eventId, TimeSpan eventDuration)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
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
        public async Task UpdateCategoryId(Guid eventId, ulong categoryId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
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
        public async Task UpdateTextChannelId(Guid eventId, ulong textChannelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
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
        public async Task UpdateVoiceChannelId(Guid eventId, ulong voiceChannelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
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
        public async Task UpdateControlPanelId(Guid eventId, ulong controlPanelId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.ControlChannel = controlPanelId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the Event Steward rank.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="stewardRankId">The Id of the steward rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateStewardRankId(Guid eventId, ulong stewardRankId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.StewardRank = stewardRankId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the speaker rank.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="speakerRankId">The Id of the Speaker Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateSpeakerRankId(Guid eventId, ulong speakerRankId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.SpeakerRank = speakerRankId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update Id of the Attendee Role.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="attendeeRankId">The Id of the attendee Role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAttendeeRankId(Guid eventId, ulong attendeeRankId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.AttendeeRank = attendeeRankId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the Id of the Cosmetic rank of an event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="cosmeticRankId">The Id of the cosmetic rank.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateCosmeticRankId(Guid eventId, ulong cosmeticRankId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
            if (eventToUpdate is null)
            {
                return;
            }

            eventToUpdate.CosmeticRank = cosmeticRankId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the completion status of the event.
        /// </summary>
        /// <param name="eventId">The Id of the event.</param>
        /// <param name="eventComplete">The status of if the event is completed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventCompletionStatus(Guid eventId, bool eventComplete)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToUpdate = await GetEventByGuidAsync(eventId);
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
        public async Task DeleteEvent(Guid eventId)
        {
            using var context = _contextFactory.CreateDbContext();

            var eventToDelete = await GetEventByGuidAsync(eventId);
            if (eventToDelete is null)
            {
                return;
            }

            context.Remove(eventId);
            await context.SaveChangesAsync();
        }
    }
}