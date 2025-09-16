using CalendarAppBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarAppBackend.Repositories
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAppointmentsAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<bool> HasConflictAsync(Appointment appointment, int? excludeId = null);
        Task<Appointment> AddAsync(Appointment appointment);
        Task<Appointment?> UpdateAsync(Appointment appointment);
        Task<bool> DeleteAsync(int id);
    }
}
