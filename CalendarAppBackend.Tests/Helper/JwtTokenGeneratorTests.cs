using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CalendarAppBackend.Helpers;
using CalendarAppBackend.Models;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CalendarAppBackend.Tests.Helpers
{
    public class JwtTokenGeneratorTests
    {
        private JwtSettings GetTestSettings() => new JwtSettings
        {
            Key = "this_is_a_secure_test_key_that_is_long_enough_1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpireMinutes = 60
        };

        [Fact]
        public void GenerateToken_ShouldReturnValidToken_WithCorrectClaims()
        {
            var settings = GetTestSettings();
            var generator = new JwtTokenGenerator(settings);
            var user = new User { Id = 42, Username = "unituser" };

            var token = generator.GenerateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));

            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Equal("TestAudience", jwt.Audiences.First());

            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == "42");
            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == "unituser");

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ShouldRespectDifferentUserValues()
        {
            var settings = GetTestSettings();
            var generator = new JwtTokenGenerator(settings);
            var user = new User { Id = 99, Username = "anotheruser" };

            var token = generator.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == "99");
            Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == "anotheruser");
        }

        [Fact]
        public void GenerateToken_ShouldThrow_WhenKeyIsEmpty()
        {
            var settings = new JwtSettings
            {
                Key = "", // empty key triggers failure
                Issuer = "Issuer",
                Audience = "Audience",
                ExpireMinutes = 60
            };
            var generator = new JwtTokenGenerator(settings);
            var user = new User { Id = 1, Username = "failuser" };

            var ex = Assert.Throws<ArgumentException>(() => generator.GenerateToken(user));
            // Update the assertion to be more flexible with error codes
            Assert.True(ex.Message.Contains("IDX10703") || ex.Message.Contains("IDX10603"), 
                $"Expected error message to contain IDX10703 or IDX10603, but got: {ex.Message}");
        }
    }
}
