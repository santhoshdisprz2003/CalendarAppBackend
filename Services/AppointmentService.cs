using CalendarAppBackend.Models;
using CalendarAppBackend.Repositories;

namespace CalendarAppBackend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;

        // ✅ Keep the constant inside the class
        private const string ConflictMessage = "Appointment conflicts with an existing appointment.";

        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            return await _repository.GetAppointmentsAsync();
        }

        public async Task<Appointment?> CreateAppointmentAsync(Appointment appointment)
        {
            if (await _repository.HasConflictAsync(appointment))
            {
                throw new InvalidOperationException(ConflictMessage);
            }

            return await _repository.AddAsync(appointment);
        }

        public async Task<Appointment?> UpdateAppointmentAsync(int id, Appointment updatedAppointment)
        {
            updatedAppointment.Id = id;

            if (await _repository.HasConflictAsync(updatedAppointment, id))
            {
                throw new InvalidOperationException(ConflictMessage);
            }

            return await _repository.UpdateAsync(updatedAppointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
