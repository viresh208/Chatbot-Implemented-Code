using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.Interfaces;

public interface IAppointmentService
{
    Task<List<TimeSlot>> GetAvailableSlotsAsync(Guid doctorId, DateTime date);
    Task<List<TimeSlot>> GetAvailableSlotsByDoctorNameAsync(string doctorName);
    Task<Appointment> BookAppointmentAsync(Guid patientId, Guid doctorId, Guid clinicId, DateTime date, string slotId, string reason);
    Task<Appointment?> GetAppointmentByIdAsync(Guid id);
    Task LogAppointmentToMongoDbAsync(Appointment appointment, string patientName, string doctorName, string clinicName, Guid sessionId);
    Task<List<AppointmentSummaryDto>> GetAppointmentsFromMongoByPatientIdAsync(string patientName);
    Task CancelAppointmentInMongoAsync(Guid AppointmentId);
}
