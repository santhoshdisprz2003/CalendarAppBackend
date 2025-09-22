using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;

namespace CalendarAppBackend.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockRepo;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockRepo = new Mock<IAppointmentRepository>();
            _service = new AppointmentService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAppointmentsAsync_ShouldReturnAppointments()
        {
            var appointments = new List<Appointment> { new Appointment { Id = 1, Title = "Meeting" } };
            _mockRepo.Setup(r => r.GetAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsAsync();

            Assert.Single(result);
            Assert.Equal("Meeting", result[0].Title);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenTitleTooLong()
        {
            var appointment = new Appointment
            {
                Title = new string('A', 30),
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenDescriptionTooLong()
        {
            var appointment = new Appointment
            {
                Title = "Valid",
                Description = new string('B', 50),
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
public async Task CreateAppointmentAsync_ShouldThrow_WhenStartTimeInPast()
{
    var appointment = new Appointment
    {
        Title = "Valid",
        Description = "Valid",
        StartTime = DateTimeOffset.UtcNow.AddHours(2), 
        EndTime = DateTimeOffset.UtcNow.AddHours(3)
    };

    // Only mock repository methods, not validation
    _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(false);
    _mockRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync((Appointment a) => a);

    await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
}


        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenEndTimeBeforeStartTime()
        {
            var appointment = new Appointment
            {
                Title = "Valid",
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(2),
                EndTime = DateTimeOffset.UtcNow.AddHours(1)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenLocationTooLong()
        {
            var appointment = new Appointment
            {
                Title = "Valid",
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Location = new string('L', 15)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenAttendeesTooLong()
        {
            var appointment = new Appointment
            {
                Title = "Valid",
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Attendees = new string('A', 1001)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldAdd_WhenValid()
        {
            var appointment = new Appointment
            {
                Title = "Valid",
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            _mockRepo.Setup(r => r.HasConflictAsync(appointment,null)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.AddAsync(appointment)).ReturnsAsync(appointment);

            var result = await _service.CreateAppointmentAsync(appointment);

            Assert.NotNull(result);
            Assert.Equal("Valid", result.Title);
        }

        [Fact]
        public async Task UpdateAppointmentAsync_ShouldThrow_WhenConflictExists()
        {
            var appointment = new Appointment { Id = 1, Title = "Meeting" };
            _mockRepo.Setup(r => r.HasConflictAsync(appointment, 1)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAppointmentAsync(1, appointment));
        }

        [Fact]
        public async Task UpdateAppointmentAsync_ShouldUpdate_WhenNoConflict()
        {
            var appointment = new Appointment
            {
                Id = 1,
                Title = "Updated",
                Description = "Valid",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            _mockRepo.Setup(r => r.HasConflictAsync(appointment, 1)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.UpdateAsync(appointment)).ReturnsAsync(appointment);

            var result = await _service.UpdateAppointmentAsync(1, appointment);

            Assert.NotNull(result);
            Assert.Equal("Updated", result.Title);
        }

        [Fact]
        public async Task DeleteAppointmentAsync_ShouldReturnTrue_WhenDeleted()
        {
            _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            var result = await _service.DeleteAppointmentAsync(1);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAppointmentAsync_ShouldReturnFalse_WhenNotDeleted()
        {
            _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);
            var result = await _service.DeleteAppointmentAsync(1);
            Assert.False(result);
        }
    }
}
