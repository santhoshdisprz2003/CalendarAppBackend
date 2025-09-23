using CalendarAppBackend.Models;
using System.Threading.Tasks;

namespace CalendarAppBackend.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
    }
}
