using System.Text.Json.Serialization;

namespace HospitalChatbot.Application.DTOs;

public class SlotSearchRequest
{
    [JsonPropertyName("doctorName")]
    public string DoctorName { get; set; } = string.Empty;
}

public class SlotSearchResponse
{
    [JsonPropertyName("SlotId")]
    public int SlotId { get; set; }

    [JsonPropertyName("DoctorName")]
    public string DoctorName { get; set; } = string.Empty;

    [JsonPropertyName("SlotTime")]
    public string SlotTime { get; set; } = string.Empty;

    [JsonPropertyName("SlotDateTime")]
    public string SlotDateTime { get; set; } = string.Empty;

    [JsonPropertyName("StartDate")]
    public string StartDate { get; set; } = string.Empty;
}
