using System;

namespace Events.Data.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong OrganiserId { get; set; }
        public string EventTitle { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public ulong Category { get; set; }
        public ulong TextChannel { get; set; }
        public ulong VoiceChannel { get; set; }
        public ulong ControlChannel { get; set; }
        public ulong StewardRank { get; set; }
        public ulong SpeakerRank { get; set; }
        public ulong AttendeeRank { get; set; }
        public ulong CosmeticRank { get; set; }
        public bool IsCompleted { get; set; }
    }
}