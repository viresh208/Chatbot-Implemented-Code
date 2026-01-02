using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.Interfaces;

public interface IPatientService
{
    Task<AuthenticationResponse> AuthenticatePatientAsync(AuthenticationRequest request);
    Task<Patient?> GetPatientByIdAsync(Guid id);
}
