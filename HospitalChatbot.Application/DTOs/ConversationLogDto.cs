namespace HospitalChatbot.Application.DTOs;

public class ConversationLogDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string ConversationState { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string BotResponse { get; set; } = string.Empty;
    public List<OptionDto>? Options { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public DateTime Timestamp { get; set; }
    public int MessageNumber { get; set; }
}
