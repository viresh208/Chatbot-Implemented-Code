using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.Interfaces;

public interface IDoctorService
{
    Task<List<Doctor>> GetDoctorsByClinicAsync(Guid clinicId);
    Task<List<Doctor>> GetDoctorsByClinicNameAsync(string clinicName);
    Task<List<Doctor>> GetDoctorsBySymptomsAsync(string symptoms);
    Task<Doctor?> GetDoctorByIdAsync(Guid id);
}
