namespace HospitalChatbot.Application.DTOs;

public class ChatMessageRequest
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}
