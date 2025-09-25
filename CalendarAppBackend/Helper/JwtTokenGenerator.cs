using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CalendarAppBackend.Models;

namespace CalendarAppBackend.Helpers
{
    public class JwtTokenGenerator:IJwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(JwtSettings settings)
        {
            _settings = settings;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                // Store numeric UserId here
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // Optional: include username for convenience
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
