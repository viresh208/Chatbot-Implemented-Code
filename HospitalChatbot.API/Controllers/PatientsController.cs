using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
    {
        var response = await _patientService.AuthenticatePatientAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatient(Guid id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound(new { Message = "Patient not found" });
        }
        return Ok(patient);
    }
}
