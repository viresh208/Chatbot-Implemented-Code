using System.Text.Json.Serialization;

namespace HospitalChatbot.Application.DTOs;

public class DoctorSearchRequest
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}

public class DoctorSearchResponse
{
    [JsonPropertyName("DoctorId")]
    public int DoctorId { get; set; }

    [JsonPropertyName("DoctorName")]
    public string DoctorName { get; set; } = string.Empty;
}

public class SymptomsRequest
{
    [JsonPropertyName("Symptoms")]
    public string Symptoms { get; set; } = string.Empty;
}

public class SymptomsResponse
{
    [JsonPropertyName("DepartmentId")]
    public int DepartmentId { get; set; }

    [JsonPropertyName("DepartmentName")]
    public string DepartmentName { get; set; } = string.Empty;
}

public class DepartmentRequest
{
    [JsonPropertyName("DepartmentName")]
    public string DepartmentName { get; set; } = string.Empty;
}

public class DepartmentDoctorResponse
{
    [JsonPropertyName("doctor Id")]
    public int DoctorId { get; set; }

    [JsonPropertyName("doctorName")]
    public string DoctorName { get; set; } = string.Empty;
}
