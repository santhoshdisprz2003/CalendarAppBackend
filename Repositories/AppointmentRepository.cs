using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CalendarAppBackend.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        public async Task<bool> HasConflictAsync(Appointment appointment, int? excludeId = null)
        {
            return await _context.Appointments.AnyAsync(a =>
                (excludeId == null || a.Id != excludeId) &&
                appointment.StartTime < a.EndTime &&
                appointment.EndTime > a.StartTime
            );
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment?> UpdateAsync(Appointment appointment)
        {
            var existing = await _context.Appointments.FindAsync(appointment.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(appointment);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
