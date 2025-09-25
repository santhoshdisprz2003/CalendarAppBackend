using CalendarAppBackend.Models;

namespace CalendarAppBackend.Helpers
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
