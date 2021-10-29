namespace Events.Data.Models
{
    using System;

    /// <summary>
    /// The event that is being run in the guild.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets or Sets the Id of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the guild the event is running in.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the Organiser of the event.
        /// </summary>
        public ulong OrganiserId { get; set; }

        /// <summary>
        /// Gets or Sets the title of the event.
        /// </summary>
        public string EventTitle { get; set; }

        /// <summary>
        /// Gets or Sets the StartTime for the event.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or Sets the duration of the event.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or Sets the Category Id of the event.
        /// </summary>
        public ulong Category { get; set; }

        /// <summary>
        /// Gets or Sets the text channel for the event.
        /// </summary>
        public ulong TextChannel { get; set; }

        /// <summary>
        /// Gets or Sets the voice channel for the event.
        /// </summary>
        public ulong VoiceChannel { get; set; }

        /// <summary>
        /// Gets or Sets the Admin Control Panel Channel for the event.
        /// </summary>
        public ulong ControlChannel { get; set; }

        /// <summary>
        /// Gets or Sets the Steward (Mod) Rank for the event.
        /// </summary>
        public ulong StewardRank { get; set; }

        /// <summary>
        /// Gets or Sets the Speaker Rank for the event.
        /// </summary>
        public ulong SpeakerRank { get; set; }

        /// <summary>
        /// Gets or Sets the Attendee Rank for the event.
        /// </summary>
        public ulong AttendeeRank { get; set; }

        /// <summary>
        /// Gets or Sets the Cosmetic Rank for the event.
        /// </summary>
        public ulong CosmeticRank { get; set; }

        /// <summary>
        /// Gets or Sets the status of if the event is completed or not.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}