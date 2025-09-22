using Xunit;
using Moq;
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
            // Arrange
            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, Title = "Test Appointment in the Appointment" }
            };
            _mockService.Setup(s => s.GetAppointmentsAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _controller.GetAppointments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnAppointments = Assert.IsType<List<Appointment>>(okResult.Value);
            Assert.Single(returnAppointments);
        }

        [Fact]
        public async Task CreateAppointment_ShouldReturnCreated_WhenSuccessful()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, Title = "New Appointment" };
            _mockService.Setup(s => s.CreateAppointmentAsync(It.IsAny<Appointment>()))
                        .ReturnsAsync(appointment);

            // Act
            var result = await _controller.CreateAppointment(appointment);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returnAppointment = Assert.IsType<Appointment>(createdResult.Value);
            Assert.Equal("New Appointment", returnAppointment.Title);
        }

        [Fact]
        public async Task UpdateAppointment_ShouldReturnOk_WhenUpdated()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, Title = "Updated Appointment" };
            _mockService.Setup(s => s.UpdateAppointmentAsync(1, It.IsAny<Appointment>()))
                        .ReturnsAsync(appointment);

            // Act
            var result = await _controller.UpdateAppointment(1, appointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("Updated Appointment", returnAppointment.Title);
        }

        [Fact]
        public async Task DeleteAppointment_ShouldReturnNoContent_WhenDeleted()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAppointmentAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAppointment(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
