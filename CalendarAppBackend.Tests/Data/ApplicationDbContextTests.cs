using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "testuser", PasswordHash = "hash" };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.Username);
            Assert.Equal("hash", savedUser.PasswordHash);
        }

        [Fact]
        public async Task Can_Update_User()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "updateuser", PasswordHash = "oldhash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            user.PasswordHash = "newhash";
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Assert
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "updateuser");
            Assert.NotNull(updatedUser);
            Assert.Equal("newhash", updatedUser.PasswordHash);
        }

        [Fact]
        public async Task Can_Delete_User()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "deleteuser", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            // Assert
            var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "deleteuser");
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task Can_Add_And_Retrieve_Appointment()
        {
            // Arrange
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

            // Act
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.Appointments.Include(a => a.User).FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("Meeting", saved.Title);
            Assert.Equal("Team meeting", saved.Description);
            Assert.Equal(user.Id, saved.UserId);
            Assert.Equal(user.Username, saved.User.Username);
        }

        [Fact]
        public async Task Can_Update_Appointment()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "u2", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = DateTimeOffset.UtcNow.AddHours(2);
            var appointment = new Appointment
            {
                Title = "Original Title",
                Description = "Original Description",
                StartTime = startTime,
                EndTime = endTime,
                UserId = user.Id
            };
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Act
            appointment.Title = "Updated Title";
            appointment.Description = "Updated Description";
            appointment.StartTime = startTime.AddHours(1);
            appointment.EndTime = endTime.AddHours(1);
            context.Appointments.Update(appointment);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Description", updated.Description);
            Assert.Equal(startTime.AddHours(1), updated.StartTime);
            Assert.Equal(endTime.AddHours(1), updated.EndTime);
        }

        [Fact]
        public async Task Can_Delete_Appointment()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "u3", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var appointment = new Appointment
            {
                Title = "Delete Me",
                Description = "To be deleted",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                UserId = user.Id
            };
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Act
            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();

            // Assert
            var deleted = await context.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task Cascade_Delete_Works_For_User_Appointments()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "u4", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var appointment1 = new Appointment
            {
                Title = "Call",
                Description = "Client call",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                UserId = user.Id
            };
            var appointment2 = new Appointment
            {
                Title = "Meeting",
                Description = "Team meeting",
                StartTime = DateTimeOffset.UtcNow.AddHours(3),
                EndTime = DateTimeOffset.UtcNow.AddHours(4),
                UserId = user.Id
            };
            context.Appointments.AddRange(appointment1, appointment2);
            await context.SaveChangesAsync();

            // Act
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            // Assert
            var remainingAppointments = await context.Appointments.Where(a => a.UserId == user.Id).ToListAsync();
            Assert.Empty(remainingAppointments);
        }

        [Fact]
        public async Task Appointment_Start_EndTime_Are_Utc()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "utcuser", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var start = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var end = new DateTimeOffset(2023, 1, 1, 13, 0, 0, TimeSpan.Zero);

            var appointment = new Appointment
            {
                Title = "UTC Test",
                Description = "Check UTC",
                StartTime = start,
                EndTime = end,
                UserId = user.Id
            };

            // Act
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.Appointments.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(DateTimeKind.Utc, saved.StartTime.UtcDateTime.Kind);
            Assert.Equal(DateTimeKind.Utc, saved.EndTime.UtcDateTime.Kind);
            Assert.Equal(start.UtcDateTime, saved.StartTime.UtcDateTime);
            Assert.Equal(end.UtcDateTime, saved.EndTime.UtcDateTime);
        }

        [Fact]
        public async Task Can_Query_Appointments_By_User()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user1 = new User { Username = "user1", PasswordHash = "hash1" };
            var user2 = new User { Username = "user2", PasswordHash = "hash2" };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Title = "User1 Meeting1",
                    Description = "First meeting",
                    StartTime = DateTimeOffset.UtcNow.AddHours(1),
                    EndTime = DateTimeOffset.UtcNow.AddHours(2),
                    UserId = user1.Id
                },
                new Appointment
                {
                    Title = "User1 Meeting2",
                    Description = "Second meeting",
                    StartTime = DateTimeOffset.UtcNow.AddHours(3),
                    EndTime = DateTimeOffset.UtcNow.AddHours(4),
                    UserId = user1.Id
                },
                new Appointment
                {
                    Title = "User2 Meeting",
                    Description = "Other user meeting",
                    StartTime = DateTimeOffset.UtcNow.AddHours(1),
                    EndTime = DateTimeOffset.UtcNow.AddHours(2),
                    UserId = user2.Id
                }
            };
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            // Act
            var user1Appointments = await context.Appointments
                .Where(a => a.UserId == user1.Id)
                .ToListAsync();

            var user2Appointments = await context.Appointments
                .Where(a => a.UserId == user2.Id)
                .ToListAsync();

            // Assert
            Assert.Equal(2, user1Appointments.Count);
            Assert.All(user1Appointments, a => Assert.Equal(user1.Id, a.UserId));
            Assert.Contains(user1Appointments, a => a.Title == "User1 Meeting1");
            Assert.Contains(user1Appointments, a => a.Title == "User1 Meeting2");

            Assert.Single(user2Appointments);
            Assert.All(user2Appointments, a => Assert.Equal(user2.Id, a.UserId));
            Assert.Contains(user2Appointments, a => a.Title == "User2 Meeting");
        }

        [Fact]
        public async Task Can_Query_Appointments_By_TimeRange()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "timeuser", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var now = DateTimeOffset.UtcNow;
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Title = "Past Meeting",
                    Description = "Already happened",
                    StartTime = now.AddDays(-2),
                    EndTime = now.AddDays(-2).AddHours(1),
                    UserId = user.Id
                },
                new Appointment
                {
                    Title = "Current Meeting",
                    Description = "Happening now",
                    StartTime = now.AddHours(-1),
                    EndTime = now.AddHours(1),
                    UserId = user.Id
                },
                new Appointment
                {
                    Title = "Future Meeting",
                    Description = "Coming up",
                    StartTime = now.AddDays(2),
                    EndTime = now.AddDays(2).AddHours(1),
                    UserId = user.Id
                }
            };
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            // Act
            var futureAppointments = await context.Appointments
                .Where(a => a.StartTime > now)
                .ToListAsync();

            var pastAppointments = await context.Appointments
                .Where(a => a.EndTime < now)
                .ToListAsync();

            var currentAppointments = await context.Appointments
                .Where(a => a.StartTime <= now && a.EndTime >= now)
                .ToListAsync();

            // Assert
            Assert.Single(futureAppointments);
            Assert.Equal("Future Meeting", futureAppointments[0].Title);

            Assert.Single(pastAppointments);
            Assert.Equal("Past Meeting", pastAppointments[0].Title);

            Assert.Single(currentAppointments);
            Assert.Equal("Current Meeting", currentAppointments[0].Title);
        }

        [Fact]
        public void Table_Name_Is_Appointments()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var entityType = context.Model.FindEntityType(typeof(Appointment));
            
            // Assert
            Assert.Equal("Appointments", entityType.GetTableName());
        }

        [Fact]
        public void Table_Name_Is_Users()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var entityType = context.Model.FindEntityType(typeof(User));
            
            // Assert
            Assert.Equal("Users", entityType.GetTableName());
        }

        [Fact]
        public void OnModelCreating_ConfiguresAppointmentUserRelationship()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var appointmentEntity = context.Model.FindEntityType(typeof(Appointment));
            var navigation = appointmentEntity.FindNavigation(nameof(Appointment.User));
            var fk = navigation.ForeignKey;
            
            // Assert
            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
            Assert.Equal(nameof(Appointment.UserId), fk.Properties.First().Name);
        }

        [Fact]
        public void OnModelCreating_ConfiguresDateTimeConversions()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var appointmentEntity = context.Model.FindEntityType(typeof(Appointment));
            var startTimeProperty = appointmentEntity.FindProperty(nameof(Appointment.StartTime));
            var endTimeProperty = appointmentEntity.FindProperty(nameof(Appointment.EndTime));
            
            // Assert
            Assert.NotNull(startTimeProperty.GetValueConverter());
            Assert.NotNull(endTimeProperty.GetValueConverter());
        }

        [Fact]
        public async Task DbSets_AreNotNull()
        {
            // Arrange & Act
            var context = await GetDbContextAsync();
            
            // Assert
            Assert.NotNull(context.Users);
            Assert.NotNull(context.Appointments);
        }

        [Fact]
        public async Task Can_Add_Multiple_Users()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var users = new List<User>
            {
                new User { Username = "user1", PasswordHash = "hash1" },
                new User { Username = "user2", PasswordHash = "hash2" },
                new User { Username = "user3", PasswordHash = "hash3" }
            };

            // Act
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Assert
            var savedUsers = await context.Users.ToListAsync();
            Assert.Equal(3, savedUsers.Count);
            Assert.Contains(savedUsers, u => u.Username == "user1");
            Assert.Contains(savedUsers, u => u.Username == "user2");
            Assert.Contains(savedUsers, u => u.Username == "user3");
        }

        [Fact]
        public async Task Can_Add_Multiple_Appointments()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var user = new User { Username = "multiuser", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Title = "Meeting 1",
                    Description = "First meeting",
                    StartTime = DateTimeOffset.UtcNow.AddHours(1),
                    EndTime = DateTimeOffset.UtcNow.AddHours(2),
                    UserId = user.Id
                },
                new Appointment
                {
                    Title = "Meeting 2",
                    Description = "Second meeting",
                    StartTime = DateTimeOffset.UtcNow.AddHours(3),
                    EndTime = DateTimeOffset.UtcNow.AddHours(4),
                    UserId = user.Id
                }
            };

            // Act
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            // Assert
            var savedAppointments = await context.Appointments.ToListAsync();
            Assert.Equal(2, savedAppointments.Count);
            Assert.Contains(savedAppointments, a => a.Title == "Meeting 1");
            Assert.Contains(savedAppointments, a => a.Title == "Meeting 2");
        }
    }
}
