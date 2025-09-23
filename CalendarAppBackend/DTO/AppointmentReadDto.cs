namespace CalendarAppBackend.DTO
{
    public class AppointmentReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Attendees { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
    