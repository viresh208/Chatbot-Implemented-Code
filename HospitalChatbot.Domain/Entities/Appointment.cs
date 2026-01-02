namespace HospitalChatbot.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TimeSlot
{
    public string SlotId { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}
