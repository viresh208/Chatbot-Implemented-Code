using HospitalChatbot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetDoctorsByClinic(Guid clinicId)
    {
        var doctors = await _doctorService.GetDoctorsByClinicAsync(clinicId);
        return Ok(doctors);
    }

    [HttpGet("symptoms")]
    public async Task<IActionResult> GetDoctorsBySymptoms([FromQuery] string symptoms)
    {
        if (string.IsNullOrWhiteSpace(symptoms))
        {
            return BadRequest(new { Message = "Symptoms parameter is required" });
        }

        var doctors = await _doctorService.GetDoctorsBySymptomsAsync(symptoms);
        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctor(Guid id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null)
        {
            return NotFound(new { Message = "Doctor not found" });
        }
        return Ok(doctor);
    }
}
