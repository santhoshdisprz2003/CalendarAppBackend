using System.Threading.Tasks;
using Xunit;
using Moq;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;
using CalendarAppBackend.Services;
using CalendarAppBackend.Helpers;
using Microsoft.AspNetCore.Identity;

namespace CalendarAppBackend.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtTokenGenerator> _jwtMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtMock = new Mock<IJwtTokenGenerator>();

            _authService = new AuthService(_userRepoMock.Object, _jwtMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = "testuser",
                PasswordHash = hasher.HashPassword(new User(), "password123")
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser"))
                         .ReturnsAsync(user);

            _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

            var request = new LoginRequest { Username = "testuser", Password = "password123" };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            _userRepoMock.Setup(r => r.GetByUsernameAsync("missing"))
                         .ReturnsAsync((User?)null);

            var request = new LoginRequest { Username = "missing", Password = "irrelevant" };

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordHashIsEmpty()
        {
            var user = new User { Username = "test", PasswordHash = "" };
            _userRepoMock.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync(user);

            var request = new LoginRequest { Username = "test", Password = "password123" };

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordDoesNotMatch()
        {
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = "testuser",
                PasswordHash = hasher.HashPassword(new User(), "correctpassword")
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(user);

            var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnTrue_WhenUserIsNew()
        {
            _userRepoMock.Setup(r => r.GetByUsernameAsync("newuser"))
                         .ReturnsAsync((User?)null);

            _userRepoMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            var request = new RegisterRequest { Username = "newuser", Password = "password123" };

            var result = await _authService.RegisterAsync(request);

            Assert.True(result);
            _userRepoMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            var existingUser = new User { Username = "existing" };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("existing"))
                         .ReturnsAsync(existingUser);

            var request = new RegisterRequest { Username = "existing", Password = "irrelevant" };

            var result = await _authService.RegisterAsync(request);

            Assert.False(result);
            _userRepoMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
