using Xunit;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;
using System.Linq;

namespace CalendarAppBackend.Tests.Repositories
{
    public class AppointmentRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentRepository _repository;
        private readonly User _testUser;

        public AppointmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new AppointmentRepository(_context);

            // Seed a test user
            _testUser = new User { Id = 1, Username = "testuser", PasswordHash = "hash" };
            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        // ================= AddAsync =================
        [Fact]
        public async Task AddAsync_ShouldSaveAppointment()
        {
            var appointment = new Appointment
            {
                Title = "Repo Test",
                Description = "Valid description",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };

            var result = await _repository.AddAsync(appointment);

            Assert.NotNull(result);
            Assert.Equal("Repo Test", result.Title);
        }

        // ================= GetAppointmentsAsync =================
        [Fact]
        public async Task GetAppointmentsAsync_ShouldReturnAllAppointments()
        {
            _context.Appointments.AddRange(
                new Appointment
                {
                    Title = "Test 1",
                    Description = "Desc 1",
                    StartTime = DateTimeOffset.Now,
                    EndTime = DateTimeOffset.Now.AddHours(1),
                    UserId = _testUser.Id
                },
                new Appointment
                {
                    Title = "Test 2",
                    Description = "Desc 2",
                    StartTime = DateTimeOffset.Now.AddHours(2),
                    EndTime = DateTimeOffset.Now.AddHours(3),
                    UserId = _testUser.Id
                }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetAppointmentsAsync();
            Assert.Equal(2, result.Count);
        }

        // ================= GetByIdAsync =================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnAppointment_WhenExists()
        {
            var appointment = new Appointment
            {
                Title = "Find Me",
                Description = "Valid description",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(appointment.Id);

            Assert.NotNull(result);
            Assert.Equal("Find Me", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _repository.GetByIdAsync(999);
            Assert.Null(result);
        }

        // ================= HasConflictAsync =================
        [Fact]
        public async Task HasConflictAsync_ShouldReturnTrue_WhenOverlapExists()
        {
            var existing = new Appointment
            {
                Title = "Existing",
                Description = "Existing",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(2),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(existing);
            await _context.SaveChangesAsync();

            var newAppointment = new Appointment
            {
                Title = "Overlap",
                Description = "Overlap",
                StartTime = DateTimeOffset.Now.AddMinutes(30),
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };

            var result = await _repository.HasConflictAsync(newAppointment);
            Assert.True(result);
        }

        [Fact]
        public async Task HasConflictAsync_ShouldReturnFalse_WhenNoOverlap()
        {
            var existing = new Appointment
            {
                Title = "Existing",
                Description = "Existing",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(existing);
            await _context.SaveChangesAsync();

            var newAppointment = new Appointment
            {
                Title = "No Conflict",
                Description = "No Conflict",
                StartTime = DateTimeOffset.Now.AddHours(2),
                EndTime = DateTimeOffset.Now.AddHours(3),
                UserId = _testUser.Id
            };

            var result = await _repository.HasConflictAsync(newAppointment);
            Assert.False(result);
        }

        [Fact]
        public async Task HasConflictAsync_ShouldRespectExcludeId()
        {
            var existing = new Appointment
            {
                Title = "Existing",
                Description = "Existing",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(existing);
            await _context.SaveChangesAsync();

            var newAppointment = new Appointment
            {
                Title = "Exclude Test",
                Description = "Exclude",
                StartTime = existing.StartTime,
                EndTime = existing.EndTime,
                UserId = _testUser.Id
            };

            var result = await _repository.HasConflictAsync(newAppointment, existing.Id);
            Assert.False(result); // excludeId branch
        }

        // ================= UpdateAsync =================
        [Fact]
        public async Task UpdateAsync_ShouldUpdate_WhenAppointmentExists()
        {
            var appointment = new Appointment
            {
                Title = "Before",
                Description = "Before",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            appointment.Title = "After";
            appointment.Description = "After";

            var result = await _repository.UpdateAsync(appointment);
            Assert.NotNull(result);
            Assert.Equal("After", result.Title);
            Assert.Equal("After", result.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _repository.UpdateAsync(new Appointment { Id = 999, UserId = _testUser.Id });
            Assert.Null(result);
        }

        // ================= DeleteAsync =================
        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
        {
            var appointment = new Appointment
            {
                Title = "Delete Me",
                Description = "Delete",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(1),
                UserId = _testUser.Id
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(appointment.Id);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var result = await _repository.DeleteAsync(999);
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
