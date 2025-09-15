namespace CalendarAppBackend.Models
{
    public class Appointment
    {
        public int Id { get; set; }             // PK
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } // datetime
        public DateTime EndTime { get; set; }   // datetime

        // ✅ Add missing properties
        public bool IsAllDay { get; set; } = false;
        public string Location { get; set; } = string.Empty;
        public string Attendees { get; set; } = string.Empty; // store as comma-separated list, or make a separate table if needed
    }
}
