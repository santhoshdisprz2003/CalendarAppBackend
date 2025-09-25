using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CalendarAppBackend.Controllers;
using CalendarAppBackend.Data;
using CalendarAppBackend.DTO;
using CalendarAppBackend.Helpers;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CalendarAppBackend.Tests.Integration
{
    public class IntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly AppointmentsController _appointmentsController;

        public IntegrationTests()
        {
            // Setup in-memory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Repositories & services
            _userRepository = new UserRepository(_context);
            var jwtSettings = new JwtSettings { Key = "dummykey1234567890", Issuer = "test", Audience = "test", ExpireMinutes = 60 };
            _authService = new AuthService(_userRepository, new JwtTokenGenerator(jwtSettings));
            _appointmentRepository = new AppointmentRepository(_context);
            _appointmentService = new AppointmentService(_appointmentRepository);

            // Seed a test user
            var user = new User { Username = "tester", PasswordHash = "hashed" };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Setup controller with fake user identity
            _appointmentsController = new AppointmentsController(_appointmentService);
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "mock"));

            _appointmentsController.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = httpContext
            };

            // Force OnModelCreating coverage
            var model = _context.Model;
        }

        [Fact]
        public async Task Full_CRUD_Appointment_Test()
        {
            // --- CREATE ---
            var createDto = new AppointmentCreateDto
            {
                Title = "Meeting",
                Description = "Integration Test",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Location = "Office",
                Attendees = "John",
                IsAllDay = false
            };

            var createResult = await _appointmentsController.CreateAppointment(createDto);
            var created = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(createResult);
            Assert.Equal(201, created.StatusCode);
            var createdAppointment = Assert.IsType<Appointment>(created.Value);
            Assert.Equal("Meeting", createdAppointment.Title);

            // --- GET ---
            var getResult = await _appointmentsController.GetAppointments();
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(getResult);
            var appointments = Assert.IsAssignableFrom<IEnumerable<AppointmentReadDto>>(okResult.Value);
            Assert.Single(appointments);

            // --- UPDATE ---
            var updateDto = new AppointmentUpdateDto
            {
                Title = "Updated Meeting",
                Description = "Updated Desc",
                StartTime = createdAppointment.StartTime,
                EndTime = createdAppointment.EndTime,
                Location = "New Office",
                Attendees = "John, Jane",
                IsAllDay = false
            };

            var updateResult = await _appointmentsController.UpdateAppointment(createdAppointment.Id, updateDto);
            var updatedOk = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(updateResult);
            var updatedAppointment = Assert.IsType<Appointment>(updatedOk.Value);
            Assert.Equal("Updated Meeting", updatedAppointment.Title);
            Assert.Equal("New Office", updatedAppointment.Location);

            // --- DELETE ---
            var deleteResult = await _appointmentsController.DeleteAppointment(createdAppointment.Id);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(deleteResult);

            // Confirm deleted
            var postDelete = await _appointmentsController.GetAppointments();
            var afterDelete = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(postDelete);
            var listAfterDelete = Assert.IsAssignableFrom<IEnumerable<AppointmentReadDto>>(afterDelete.Value);
            Assert.Empty(listAfterDelete);
        }

        [Fact]
        public void ApplicationDbContext_Coverage()
        {
            // Constructor coverage
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            Assert.NotNull(context);

            // Access DbSets
            Assert.NotNull(context.Users);
            Assert.NotNull(context.Appointments);

            // OnModelCreating coverage via reflection
            var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());
            context.GetType()
                .GetMethod("OnModelCreating", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(context, new object[] { modelBuilder });

            var entity = modelBuilder.Model.FindEntityType(typeof(Appointment));
            Assert.Equal("Appointments", entity.GetTableName());
            Assert.NotNull(entity.FindNavigation(nameof(Appointment.User)));
            Assert.NotNull(entity.FindProperty(nameof(Appointment.StartTime)).GetValueConverter());
            Assert.NotNull(entity.FindProperty(nameof(Appointment.EndTime)).GetValueConverter());
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
