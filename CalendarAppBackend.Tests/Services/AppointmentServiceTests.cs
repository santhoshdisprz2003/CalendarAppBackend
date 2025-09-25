using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalendarAppBackend.Models;
using CalendarAppBackend.Services;
using CalendarAppBackend.Repositories;

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

        private Appointment GetValidAppointment() =>
            new Appointment
            {
                Title = "Valid Title",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                UserId = 1
            };

        [Fact]
        public async Task GetAppointmentsByUserAsync_ShouldReturnAppointments()
        {
            var appointments = new List<Appointment> { GetValidAppointment() };
            _mockRepo.Setup(r => r.GetAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByUserAsync(1);

            Assert.Single(result);
            Assert.Equal("Valid Title", result.First().Title);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldAdd_WhenValid()
        {
            var appointment = GetValidAppointment();

            _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.AddAsync(appointment)).ReturnsAsync(appointment);

            var result = await _service.CreateAppointmentAsync(appointment);

            Assert.NotNull(result);
            Assert.Equal("Valid Title", result.Title);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenConflictExists()
        {
            var appointment = GetValidAppointment();

            _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task UpdateAppointmentForUserAsync_ShouldUpdate_WhenNoConflict()
        {
            var appointment = GetValidAppointment();
            appointment.Id = 1;

            _mockRepo.Setup(r => r.HasConflictAsync(appointment, 1)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.UpdateAsync(appointment)).ReturnsAsync(appointment);

            var result = await _service.UpdateAppointmentForUserAsync(1, 1, appointment);

            Assert.NotNull(result);
            Assert.Equal("Valid Title", result.Title);
        }

        [Fact]
        public async Task UpdateAppointmentForUserAsync_ShouldThrow_WhenConflictExists()
        {
            var appointment = GetValidAppointment();
            appointment.Id = 1;

            _mockRepo.Setup(r => r.HasConflictAsync(appointment, 1)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAppointmentForUserAsync(1, 1, appointment));
        }

        [Fact]
        public async Task DeleteAppointmentForUserAsync_ShouldReturnTrue_WhenDeleted()
        {
            _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteAppointmentForUserAsync(1, 1);

            Assert.True(result);
        }


        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenStartTimeIsPast()
        {
            var appointment = GetValidAppointment();
            appointment.StartTime = DateTimeOffset.UtcNow.AddHours(-1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenEndTimeBeforeStartTime()
        {
            var appointment = GetValidAppointment();
            appointment.EndTime = appointment.StartTime.AddMinutes(-30);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenTitleTooLong()
        {
            var appointment = GetValidAppointment();
            appointment.Title = new string('A', 31); // > 30 chars

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenDescriptionTooLong()
        {
            var appointment = GetValidAppointment();
            appointment.Description = new string('B', 51); // > 50 chars

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }

        [Fact]
        public async Task CreateAppointmentAsync_ShouldThrow_WhenConflictAppointmentsExist()
        {
            var appointment = GetValidAppointment();
            _mockRepo.Setup(r => r.HasConflictAsync(appointment, null)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(appointment));
        }
    }
}
