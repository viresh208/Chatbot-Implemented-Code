using System.Text.Json.Serialization;

namespace HospitalChatbot.Application.DTOs;

public class PatientLoginRequest
{
    [JsonPropertyName("DateofBirth")]
    public string DateofBirth { get; set; } = string.Empty;
}

public class PatientLoginResponse
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("DateofBirth")]
    public string DateofBirth { get; set; } = string.Empty;

    [JsonPropertyName("Mobile Number")]
    public string MobileNumber { get; set; } = string.Empty;
}
