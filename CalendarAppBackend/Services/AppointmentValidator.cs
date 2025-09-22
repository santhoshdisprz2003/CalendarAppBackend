using CalendarAppBackend.Models;
using System;

namespace CalendarAppBackend.Services
{
    public static class AppointmentValidator
    {
        public static void Validate(Appointment appointment)
        {
            if (appointment == null)
                throw new InvalidOperationException("Appointment cannot be null.");

            // Title validation
            if (string.IsNullOrWhiteSpace(appointment.Title))
                throw new InvalidOperationException("Title is required.");
            if (appointment.Title.Length > 30)
                throw new InvalidOperationException("Title cannot exceed 30 characters.");

            //  Description validation
            if (string.IsNullOrWhiteSpace(appointment.Description))
                throw new InvalidOperationException("Description is required.");
            if (appointment.Description.Length > 50)
                throw new InvalidOperationException("Description cannot exceed 50 characters.");

            // StartTime validation
            if (appointment.StartTime == default)
                throw new InvalidOperationException("Start time is required.");
            if (appointment.StartTime < DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Start time cannot be in the past.");

            //  EndTime validation
            if (appointment.EndTime == default)
                throw new InvalidOperationException("End time is required.");
            if (appointment.EndTime <= appointment.StartTime)
                throw new InvalidOperationException("End time must be greater than start time.");

            // Location validation (optional)
            if (!string.IsNullOrWhiteSpace(appointment.Location) && appointment.Location.Length > 15)
                throw new InvalidOperationException("Location cannot exceed 15 characters.");

            //  Attendees validation (optional)
            if (!string.IsNullOrWhiteSpace(appointment.Attendees) && appointment.Attendees.Length > 1000)
                throw new InvalidOperationException("Attendees field is too long.");
        }
    }
}
