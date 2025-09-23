using CalendarAppBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarAppBackend.Services
{
    public interface IAppointmentService
    {


        Task<IEnumerable<Appointment>> GetAppointmentsByUserAsync(int userId);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> UpdateAppointmentForUserAsync(int id, int userId, Appointment appointment);
        Task<bool> DeleteAppointmentForUserAsync(int id, int userId);
    }
}
