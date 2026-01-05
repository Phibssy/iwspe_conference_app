using System;

namespace Conference.Functions.Models
{
    public class EventSelection
    {
        public string EventId { get; set; }
        public string Status { get; set; } // "confirmed", "waitlisted"
        public string RegisteredAt { get; set; }
    }
}