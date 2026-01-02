using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.Interfaces;

public interface IClinicService
{
    Task<List<Clinic>> GetAllClinicsAsync();
    Task<Clinic?> GetClinicByIdAsync(Guid id);
    Task<Clinic?> GetClinicByPatientNameAsync(string patientName);
}
