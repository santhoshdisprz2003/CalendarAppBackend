using Xunit;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;

namespace CalendarAppBackend.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUserSuccessfully()
        {
            var user = new User
            {
                Username = "testuser"
            };

            await _repository.AddUserAsync(user);

            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(addedUser);
            Assert.Equal("testuser", addedUser.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
        {
            var user = new User
            {
                Username = "existinguser"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByUsernameAsync("existinguser");

            Assert.NotNull(result);
            Assert.Equal("existinguser", result.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var result = await _repository.GetByUsernameAsync("nonexistent");
            Assert.Null(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
