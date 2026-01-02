using HospitalChatbot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly IClinicService _clinicService;

    public ClinicsController(IClinicService clinicService)
    {
        _clinicService = clinicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllClinics()
    {
        var clinics = await _clinicService.GetAllClinicsAsync();
        return Ok(clinics);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClinic(Guid id)
    {
        var clinic = await _clinicService.GetClinicByIdAsync(id);
        if (clinic == null)
        {
            return NotFound(new { Message = "Clinic not found" });
        }
        return Ok(clinic);
    }
}
