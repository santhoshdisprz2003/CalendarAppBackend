using CalendarAppBackend.Models;
using CalendarAppBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var success = await _authService.RegisterAsync(request);
            if (!success) return BadRequest("Username already exists");

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = await _authService.LoginAsync(request);
            if (token == null) return Unauthorized("Invalid credentials");

            return Ok(new { Token = token });
        }
    }
}
