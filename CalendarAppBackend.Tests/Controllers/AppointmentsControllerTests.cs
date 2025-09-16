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
                new Appointment { Id = 1, Title = "Test Appointment" }
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
        public async Task DeleteAppointment_ShouldReturnNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAppointmentAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteAppointment(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
