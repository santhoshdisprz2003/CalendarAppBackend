using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CalendarAppBackend.Tests.Data
{
    public class ApplicationDbContextTests
    {
        private async Task<ApplicationDbContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        [Fact]
        public async Task Can_Add_And_Retrieve_User()
        {
            var context = await GetDbContextAsync();
            var user = new User { Username = "testuser", PasswordHash = "hash" };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.Username);
        }

        [Fact]
        public async Task Can_Add_And_Retrieve_Appointment()
        {
            var context = await GetDbContextAsync();
            var user = new User { Username = "u1", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var appointment = new Appointment
            {
                Title = "Meeting",
                Description = "Team meeting",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                UserId = user.Id
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            var saved = await context.Appointments.Include(a => a.User).FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("Meeting", saved.Title);
            Assert.Equal(user.Id, saved.UserId);
            Assert.Equal(user.Username, saved.User.Username);
        }

        [Fact]
        public async Task Cascade_Delete_Works_For_User_Appointments()
        {
            var context = await GetDbContextAsync();
            var user = new User { Username = "u2", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var appointment = new Appointment
            {
                Title = "Call",
                Description = "Client call",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                UserId = user.Id
            };
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            var remainingAppointments = await context.Appointments.ToListAsync();
            Assert.Empty(remainingAppointments);
        }

        [Fact]
        public async Task Appointment_Start_EndTime_Are_Utc()
        {
            var context = await GetDbContextAsync();
            var user = new User { Username = "utcuser", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var start = DateTimeOffset.Now.AddHours(1);
            var end = DateTimeOffset.Now.AddHours(2);

            var appointment = new Appointment
            {
                Title = "UTC Test",
                Description = "Check UTC",
                StartTime = start,
                EndTime = end,
                UserId = user.Id
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            var saved = await context.Appointments.FirstOrDefaultAsync();

            Assert.NotNull(saved);
            Assert.Equal(DateTimeKind.Utc, saved.StartTime.UtcDateTime.Kind);
            Assert.Equal(DateTimeKind.Utc, saved.EndTime.UtcDateTime.Kind);
        }

        [Fact]
        public void Table_Name_Is_Appointments()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);
            var entityType = context.Model.FindEntityType(typeof(Appointment));
            Assert.Equal("Appointments", entityType.GetTableName());
        }
    }
}
