using CalendarAppBackend.Models;
using CalendarAppBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var appointments = await _service.GetAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            try
            {
                var created = await _service.CreateAppointmentAsync(appointment);
                return StatusCode(201, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            try
            {
                var updated = await _service.UpdateAppointmentAsync(id, appointment);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var deleted = await _service.DeleteAppointmentAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
