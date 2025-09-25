using System;
using CalendarAppBackend.Models;
using CalendarAppBackend.Services;
using Xunit;

namespace CalendarAppBackend.Tests.Services
{
    public class AppointmentValidatorTests
    {
        private Appointment CreateValidAppointment()
        {
            return new Appointment
            {
                Id = 1,
                Title = "Valid Title",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Location = "Office",
                Attendees = "test@example.com",
                UserId = 1
            };
        }

        [Fact]
        public void Validate_ShouldPass_WhenAppointmentIsValid()
        {
            var appointment = CreateValidAppointment();

            var ex = Record.Exception(() => AppointmentValidator.Validate(appointment));

            Assert.Null(ex);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenAppointmentIsNull()
        {
            Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(null!));
        }

        [Fact]
        public void Validate_ShouldThrow_WhenTitleIsMissing()
        {
            var appointment = CreateValidAppointment();
            appointment.Title = "";

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Title is required.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenTitleTooLong()
        {
            var appointment = CreateValidAppointment();
            appointment.Title = new string('A', 31);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Title cannot exceed 30 characters.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenDescriptionIsMissing()
        {
            var appointment = CreateValidAppointment();
            appointment.Description = "";

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Description is required.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenDescriptionTooLong()
        {
            var appointment = CreateValidAppointment();
            appointment.Description = new string('B', 51);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Description cannot exceed 50 characters.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenStartTimeIsDefault()
        {
            var appointment = CreateValidAppointment();
            appointment.StartTime = default;

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Start time is required.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenStartTimeInPast()
        {
            var appointment = CreateValidAppointment();
            appointment.StartTime = DateTimeOffset.UtcNow.AddMinutes(-5);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Start time cannot be in the past.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenEndTimeIsDefault()
        {
            var appointment = CreateValidAppointment();
            appointment.EndTime = default;

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("End time is required.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenEndTimeBeforeStartTime()
        {
            var appointment = CreateValidAppointment();
            appointment.EndTime = appointment.StartTime.AddMinutes(-1);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("End time must be greater than start time.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenLocationTooLong()
        {
            var appointment = CreateValidAppointment();
            appointment.Location = new string('C', 16);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Location cannot exceed 15 characters.", ex.Message);
        }

        [Fact]
        public void Validate_ShouldThrow_WhenAttendeesTooLong()
        {
            var appointment = CreateValidAppointment();
            appointment.Attendees = new string('D', 1001);

            var ex = Assert.Throws<InvalidOperationException>(() => AppointmentValidator.Validate(appointment));
            Assert.Equal("Attendees field is too long.", ex.Message);
        }
    }
}
