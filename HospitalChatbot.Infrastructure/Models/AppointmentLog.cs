using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalChatbot.Infrastructure.Models;

public class AppointmentLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("appointmentId")]
    [BsonRepresentation(BsonType.String)]
    public Guid AppointmentId { get; set; }

    [BsonElement("patientId")]
    [BsonRepresentation(BsonType.String)]
    public Guid PatientId { get; set; }

    [BsonElement("patientName")]
    public string PatientName { get; set; } = string.Empty;

    [BsonElement("doctorId")]
    [BsonRepresentation(BsonType.String)]
    public Guid DoctorId { get; set; }

    [BsonElement("doctorName")]
    public string DoctorName { get; set; } = string.Empty;

    [BsonElement("clinicId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ClinicId { get; set; }

    [BsonElement("clinicName")]
    public string ClinicName { get; set; } = string.Empty;

    [BsonElement("appointmentDate")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    [BsonElement("timeSlot")]
    public TimeSlotMongo TimeSlot { get; set; } = new();

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SessionId { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Confirmed";

    [BsonElement("createdAt")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime? UpdatedAt { get; set; }
}

public class TimeSlotMongo
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("startTime")]
    public string StartTime { get; set; } = string.Empty;

    [BsonElement("endTime")]
    public string EndTime { get; set; } = string.Empty;

    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; }
}
