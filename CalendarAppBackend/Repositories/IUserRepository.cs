    using CalendarAppBackend.Models;
    using System.Threading.Tasks;

    namespace CalendarAppBackend.Repositories
    {
        public interface IUserRepository
        {
            Task<User?> GetByUsernameAsync(string username);
            Task AddUserAsync(User user);
        }
    }
