using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CalendarAppBackend.Controllers;
using CalendarAppBackend.Models;
using CalendarAppBackend.Services;

namespace CalendarAppBackend.Tests.Controllers
{
    public class AppointmentsControllerTests
    {
        private readonly Mock<IAppointmentService> _mockService;
        private readonly AppointmentsController _controller;

        public AppointmentsControllerTests()
        {
            _mockService = new Mock<IAppointmentService>();
            _controller = new AppointmentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetAppointments_ShouldReturnOkWithAppointments()
        {
            var appointments = new List<Appointment> { new Appointment { Id = 1, Title = "Meeting 1" } };
            _mockService.Setup(s => s.GetAppointmentsAsync()).ReturnsAsync(appointments);

            var result = await _controller.GetAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnAppointments = Assert.IsType<List<Appointment>>(okResult.Value);
            Assert.Single(returnAppointments);
        }

        [Fact]
        public async Task CreateAppointment_ShouldReturnConflict_WhenServiceThrows()
        {
            var appointment = new Appointment { Title = "Valid", Description = "Valid" };

            _mockService.Setup(s => s.CreateAppointmentAsync(It.IsAny<Appointment>()))
                        .ThrowsAsync(new InvalidOperationException("Validation failed"));

            var result = await _controller.CreateAppointment(appointment);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Validation failed", conflict.Value.ToString());
        }

        [Fact]
        public async Task UpdateAppointment_ShouldReturnOk_WhenServiceSucceeds()
        {
            var appointment = new Appointment { Id = 1, Title = "Updated", Description = "Valid" };
            _mockService.Setup(s => s.UpdateAppointmentAsync(1, It.IsAny<Appointment>()))
                        .ReturnsAsync(appointment);

            var result = await _controller.UpdateAppointment(1, appointment);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updated = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("Updated", updated.Title);
        }

        [Fact]
        public async Task UpdateAppointment_ShouldReturnConflict_WhenServiceThrows()
        {
            var appointment = new Appointment { Id = 1, Title = "Updated", Description = "Valid" };
            _mockService.Setup(s => s.UpdateAppointmentAsync(1, It.IsAny<Appointment>()))
                        .ThrowsAsync(new InvalidOperationException("Conflict occurred"));

            var result = await _controller.UpdateAppointment(1, appointment);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Conflict occurred", conflict.Value.ToString());
        }

        [Fact]
        public async Task DeleteAppointment_ShouldReturnNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAppointmentAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteAppointment(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ShouldReturnNotFound_WhenNotDeleted()
        {
            _mockService.Setup(s => s.DeleteAppointmentAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteAppointment(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
