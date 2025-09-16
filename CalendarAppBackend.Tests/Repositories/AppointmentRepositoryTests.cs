using Xunit;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;

namespace CalendarAppBackend.Tests.Repositories
{
    public class AppointmentRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentRepository _repository;

        public AppointmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new AppointmentRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldSaveAppointment()
        {
            var appointment = new Appointment { Id = 1, Title = "Repo Test" };

            var result = await _repository.AddAsync(appointment);

            Assert.NotNull(result);
            Assert.Equal("Repo Test", result.Title);
        }
    }
}
