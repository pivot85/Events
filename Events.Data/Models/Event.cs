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
        public ulong Guild { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the Organiser of the event.
        /// </summary>
        public ulong Organiser { get; set; }

        /// <summary>
        /// Gets or Sets the title of the event.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the Start for the event.
        /// </summary>
        public DateTime Start { get; set; }

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
        /// Gets or Sets the Steward (Mod) Role for the event.
        /// </summary>
        public ulong StewardRole { get; set; }

        /// <summary>
        /// Gets or Sets the Speaker Role for the event.
        /// </summary>
        public ulong SpeakerRole { get; set; }

        /// <summary>
        /// Gets or Sets the Attendee Role for the event.
        /// </summary>
        public ulong AttendeeRole { get; set; }

        /// <summary>
        /// Gets or Sets the Cosmetic Role for the event.
        /// </summary>
        public ulong CosmeticRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the status of if the event is completed or not.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}