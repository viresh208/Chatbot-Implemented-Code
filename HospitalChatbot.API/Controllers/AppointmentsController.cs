using HospitalChatbot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("slots/{doctorId}")]
    public async Task<IActionResult> GetAvailableSlots(Guid doctorId, [FromQuery] DateTime date)
    {
        if (date == default)
        {
            date = DateTime.Now.AddDays(1).Date;
        }

        var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);
        return Ok(slots);
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        try
        {
            var appointment = await _appointmentService.BookAppointmentAsync(
                request.PatientId,
                request.DoctorId,
                request.ClinicId,
                request.AppointmentDate,
                request.SlotId,
                request.Reason);

            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(new { Message = "Appointment not found" });
        }
        return Ok(appointment);
    }
}

public class BookAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string SlotId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
