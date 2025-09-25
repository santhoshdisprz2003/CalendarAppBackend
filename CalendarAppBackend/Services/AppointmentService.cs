    using CalendarAppBackend.Models;
    using CalendarAppBackend.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace CalendarAppBackend.Services
    {
        public class AppointmentService : IAppointmentService
        {
            private readonly IAppointmentRepository _repository;

            public AppointmentService(IAppointmentRepository repository)
            {
                _repository = repository;
            }

            public async Task<IEnumerable<Appointment>> GetAppointmentsByUserAsync(int userId)
            {
                if (userId <= 0) return Enumerable.Empty<Appointment>();

                var all = await _repository.GetAppointmentsAsync();
                return all.Where(a => a.UserId == userId);
            }

            public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
            {
                if (appointment == null)
                    throw new ArgumentNullException(nameof(appointment));

                if (appointment.UserId <= 0)
                    throw new InvalidOperationException("Invalid User ID");

                AppointmentValidator.Validate(appointment);

                bool conflict = await _repository.HasConflictAsync(appointment, null);
                if (conflict)
                    throw new InvalidOperationException("This appointment conflicts with an existing one.");

                return await _repository.AddAsync(appointment);
            }

            public async Task<Appointment?> UpdateAppointmentForUserAsync(int id, int userId, Appointment appointment)
            {
                if (userId <= 0) return null;

                AppointmentValidator.Validate(appointment);

                bool conflict = await _repository.HasConflictAsync(appointment, id);
                if (conflict)
                    throw new InvalidOperationException("This appointment conflicts with an existing one.");

                return await _repository.UpdateAsync(appointment);
            }

            public async Task<bool> DeleteAppointmentForUserAsync(int id, int userId)
            {
                if (userId <= 0) return false;

                return await _repository.DeleteAsync(id);
            }
        }
    }
