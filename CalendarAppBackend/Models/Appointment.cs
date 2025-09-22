namespace CalendarAppBackend.Models
{
    public class Appointment
    {
        public int Id { get; set; }            
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTimeOffset StartTime { get; set; } 
        public DateTimeOffset EndTime { get; set; }   

        public bool IsAllDay { get; set; } = false;
        public string Location { get; set; } = string.Empty;
        public string Attendees { get; set; } = string.Empty;
    }
}
