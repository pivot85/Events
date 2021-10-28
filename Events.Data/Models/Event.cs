using System;

namespace Events.Data.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public string EventTitle { get; set; }
        public ulong OrganiserId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public ulong Category { get; set; }
        public ulong TextChannel { get; set; }
        public ulong VoiceChannel { get; set; }
        public ulong ControlChannel { get; set; }
        public ulong StewardRankId { get; set; }
        public ulong SpeakerRankId { get; set; }
        public ulong AttendeeRankId { get; set; }
        public ulong CosmeticRankId { get; set; }
    }
}