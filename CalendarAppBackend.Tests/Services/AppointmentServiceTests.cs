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
            // Arrange
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, Title = "Meeting" }
            };
            _mockRepo.Setup(r => r.GetAppointmentsAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _service.GetAppointmentsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Meeting", result[0].Title);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrowException_WhenConflictExists()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, Title = "Conflict Meeting" };
            _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldAdd_WhenNoConflict()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, Title = "New Meeting" };
            _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.AddAsync(appointment)).ReturnsAsync(appointment);

            // Act
            var result = await _service.CreateAppointmentAsync(appointment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Meeting", result.Title);
        }
    }
}
