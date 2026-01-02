namespace HospitalChatbot.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public ConversationState State { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public TimeSlot timeSlot { get; set; }
}

public enum ConversationState
{
    Initial,
    AwaitingName,
    AwaitingDateOfBirth,
    Authenticated,
    AwaitingClinicSelection,
    AwaitingBookingOrCancelChoice,
    AwaitingDoctorOrSymptom,
    AwaitingDoctorSelection,
    AwaitingSlotSelection,
    BookingConfirmed,
    Completed,
    AwaitingCancellationSelection
}
