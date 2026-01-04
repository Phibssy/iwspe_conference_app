namespace Conference.Functions.Models
{
    public class EventItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // e.g., "bus", "visit", "talk"
        public string Description { get; set; }
        public string Route { get; set; }
        public string Departure { get; set; }
        public string Return { get; set; }
        public int Capacity { get; set; } = 0; // 0 = unlimited
        public int WaitlistCapacity { get; set; } = 0; // 0 = no waitlist
    }
}