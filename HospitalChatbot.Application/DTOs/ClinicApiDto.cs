using System.Text.Json.Serialization;

namespace HospitalChatbot.Application.DTOs;

public class ClinicSelectionRequest
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}

public class ClinicSelectionResponse
{
    [JsonPropertyName("ClinicId")]
    public int ClinicId { get; set; }

    [JsonPropertyName("ClinicName")]
    public string ClinicName { get; set; } = string.Empty;
}
