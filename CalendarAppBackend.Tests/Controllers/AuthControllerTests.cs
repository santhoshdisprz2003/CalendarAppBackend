using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CalendarAppBackend.Controllers;
using CalendarAppBackend.Services;
using CalendarAppBackend.Models;

namespace CalendarAppBackend.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserRegistered()
        {
            // Arrange
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(new RegisterRequest { Username = "user", Password = "pass" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully", okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Register(new RegisterRequest { Username = "existing", Password = "pass" });

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username already exists", badResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenCredentialsValid()
        {
            // Arrange
            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync("mock-token");

            // Act
            var result = await _controller.Login(new LoginRequest { Username = "user", Password = "pass" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tokenObj = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("mock-token", tokenObj.Token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
        {
            // Arrange
            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.Login(new LoginRequest { Username = "user", Password = "wrong" });

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials", unauthorizedResult.Value);
        }
    }
}
