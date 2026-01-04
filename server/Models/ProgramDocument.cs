using System.Collections.Generic;

namespace Conference.Functions.Models
{
    public class ProgramDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<EventItem> Events { get; set; }
    }
}