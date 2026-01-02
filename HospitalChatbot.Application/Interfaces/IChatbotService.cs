using HospitalChatbot.Application.DTOs;

namespace HospitalChatbot.Application.Interfaces;

public interface IChatbotService
{
    Task<ChatMessageResponse> ProcessMessageAsync(ChatMessageRequest request);
    Task<Guid> StartNewSessionAsync();
}
