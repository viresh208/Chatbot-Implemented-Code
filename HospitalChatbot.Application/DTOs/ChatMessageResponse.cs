using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.DTOs;

public class ChatMessageResponse
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public ConversationState CurrentState { get; set; }
    public List<OptionDto>? Options { get; set; }
    public bool IsCompleted { get; set; }
    public object? Data { get; set; }
}

public class OptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
