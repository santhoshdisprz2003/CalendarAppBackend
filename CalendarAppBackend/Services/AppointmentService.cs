using CalendarAppBackend.Data;
using CalendarAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CalendarAppBackend.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;

    public AppointmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all appointments for a user
    public async Task<IEnumerable<Appointment>> GetAppointmentsByUserAsync(int userId)
    {
        // Return empty list if userId is invalid
        if (userId <= 0) return Enumerable.Empty<Appointment>();

        return await _context.Appointments
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    // Create a new appointment
    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null");

        if (appointment.UserId <= 0)
            throw new InvalidOperationException("Invalid User ID");

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    // Update an existing appointment for a specific user
    public async Task<Appointment?> UpdateAppointmentForUserAsync(int id, int userId, Appointment appointment)
    {
        if (userId <= 0) return null;

        var existing = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (existing == null) return null;

        // Update only allowed fields
        existing.Title = appointment.Title;
        existing.Description = appointment.Description;
        existing.StartTime = appointment.StartTime;
        existing.EndTime = appointment.EndTime;
        existing.IsAllDay = appointment.IsAllDay;
        existing.Location = appointment.Location;
        existing.Attendees = appointment.Attendees;

        await _context.SaveChangesAsync();
        return existing;
    }

    // Delete an appointment for a specific user
    public async Task<bool> DeleteAppointmentForUserAsync(int id, int userId)
    {
        if (userId <= 0) return false;

        var existing = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (existing == null) return false;

        _context.Appointments.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
