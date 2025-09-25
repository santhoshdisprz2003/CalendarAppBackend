using CalendarAppBackend.Models;
using CalendarAppBackend.Services;
using CalendarAppBackend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CalendarAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var appointments = await _service.GetAppointmentsByUserAsync(userId);

            var result = appointments.Select(a => new AppointmentReadDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsAllDay = a.IsAllDay,
                Location = a.Location,
                Attendees = a.Attendees,
                UserId = a.UserId
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User not logged in" });
            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid user ID" });

            var appointment = new Appointment
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAllDay = dto.IsAllDay,
                Location = dto.Location,
                Attendees = dto.Attendees,
                UserId = userId
            };

            try
            {
                var created = await _service.CreateAppointmentAsync(appointment);
                return StatusCode(201, created);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("conflicts", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User not logged in" });
            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid user ID" });

            var appointment = new Appointment
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAllDay = dto.IsAllDay,
                Location = dto.Location,
                Attendees = dto.Attendees,
                UserId = userId
            };

            try
            {
                var updated = await _service.UpdateAppointmentForUserAsync(id, userId, appointment);
                if (updated == null)
                    return NotFound(new { message = "Appointment not found or not yours" });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("conflicts", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User not logged in" });
            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid user ID" });

            var deleted = await _service.DeleteAppointmentForUserAsync(id, userId);
            if (!deleted)
                return NotFound(new { message = "Appointment not found or not yours" });

            return NoContent();
        }
    }
}
