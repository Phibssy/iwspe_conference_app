namespace Conference.Functions.Models
{
    public class Registration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Affiliation { get; set; }
        public string Phone { get; set; }
        public string[] SelectedEvents { get; set; }
    }
}