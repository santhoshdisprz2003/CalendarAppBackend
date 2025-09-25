using Xunit;
using CalendarAppBackend.Helpers;

namespace CalendarAppBackend.Tests.Helpers
{
    public class JwtSettingsTests
    {
        [Fact]
        public void JwtSettings_Should_Set_And_Get_Properties()
        {
            // Arrange & Act
            var settings = new JwtSettings
            {
                Key = "supersecretkey",
                Issuer = "myissuer",
                Audience = "myaudience",
                ExpireMinutes = 60
            };

            // Assert
            Assert.Equal("supersecretkey", settings.Key);
            Assert.Equal("myissuer", settings.Issuer);
            Assert.Equal("myaudience", settings.Audience);
            Assert.Equal(60, settings.ExpireMinutes);
        }
    }
}
