using CalendarAppBackend.Helpers;
using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CalendarAppBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IUserRepository userRepository, JwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                Console.WriteLine("User not found!");
                return null;
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                Console.WriteLine("PasswordHash is empty!");
                return null;
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("Password verification failed!");
                return null;
            }
            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepository.GetByUsernameAsync(request.Username);
            if (existing != null) return false;

            var user = new User
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(new User(), request.Password)
            };

            await _userRepository.AddUserAsync(user);
            return true;
        }
    }
}
