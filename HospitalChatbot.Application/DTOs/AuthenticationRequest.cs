namespace HospitalChatbot.Application.DTOs;

public class AuthenticationRequest
{
    public string Name { get; set; } = string.Empty;
    public string DateofBirth { get; set; }
}

public class AuthenticationResponse
{
    public bool Success { get; set; }
    public Guid? PatientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
}
