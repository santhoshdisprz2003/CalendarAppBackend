using CalendarAppBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarAppBackend.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAppointmentsAsync();
        Task<Appointment?> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> UpdateAppointmentAsync(int id, Appointment updatedAppointment);
        Task<bool> DeleteAppointmentAsync(int id);
    }
}
