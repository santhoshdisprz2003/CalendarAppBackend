using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CalendarAppBackend.Controllers;
using CalendarAppBackend.Services;
using CalendarAppBackend.Models;
using CalendarAppBackend.DTO;

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

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAppointments_ShouldReturnOkWithAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment 
                { 
                    Id = 1, 
                    Title = "Meeting", 
                    Description = "Valid description",
                    StartTime = DateTimeOffset.UtcNow.AddHours(1),
                    EndTime = DateTimeOffset.UtcNow.AddHours(2),
                    UserId = 1 
                }
            };

            _mockService.Setup(s => s.GetAppointmentsByUserAsync(1)).ReturnsAsync(appointments);

            var result = await _controller.GetAppointments();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnAppointments = Assert.IsAssignableFrom<IEnumerable<AppointmentReadDto>>(okResult.Value);
            Assert.Single(returnAppointments);
        }

        [Fact]
        public async Task CreateAppointment_ShouldReturnCreated_WhenValid()
        {
            var dto = new AppointmentCreateDto
            {
                Title = "Meeting",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            var appointment = new Appointment
            {
                Id = 1,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                UserId = 1
            };

            _mockService.Setup(s => s.CreateAppointmentAsync(It.IsAny<Appointment>())).ReturnsAsync(appointment);

            var result = await _controller.CreateAppointment(dto);
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateAppointment_ShouldReturnConflict_WhenServiceThrows()
        {
            var dto = new AppointmentCreateDto
            {
                Title = "Conflict",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            _mockService.Setup(s => s.CreateAppointmentAsync(It.IsAny<Appointment>()))
                        .ThrowsAsync(new InvalidOperationException("conflicts"));

            var result = await _controller.CreateAppointment(dto);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_ShouldReturnOk_WhenServiceSucceeds()
        {
            var dto = new AppointmentUpdateDto
            {
                Title = "Updated",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            var appointment = new Appointment
            {
                Id = 1,
                Title = "Updated",
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                UserId = 1
            };

            _mockService.Setup(s => s.UpdateAppointmentForUserAsync(1, 1, It.IsAny<Appointment>())).ReturnsAsync(appointment);

            var result = await _controller.UpdateAppointment(1, dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_ShouldReturnConflict_WhenServiceThrows()
        {
            var dto = new AppointmentUpdateDto
            {
                Title = "Conflict",
                Description = "Valid description",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2)
            };

            _mockService.Setup(s => s.UpdateAppointmentForUserAsync(1, 1, It.IsAny<Appointment>()))
                        .ThrowsAsync(new InvalidOperationException("conflicts"));

            var result = await _controller.UpdateAppointment(1, dto);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ShouldReturnNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAppointmentForUserAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.DeleteAppointment(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ShouldReturnNotFound_WhenNotDeleted()
        {
            _mockService.Setup(s => s.DeleteAppointmentForUserAsync(1, 1)).ReturnsAsync(false);

            var result = await _controller.DeleteAppointment(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
