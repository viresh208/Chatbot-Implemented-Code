using HospitalChatbot.Application.DTOs;

namespace HospitalChatbot.Application.Interfaces;

public interface IConversationLogService
{
    Task LogConversationAsync(
        Guid sessionId,
        Guid? patientId,
        string? patientName,
        string conversationState,
        string userMessage,
        string botResponse,
        List<OptionDto>? options = null,
        Dictionary<string, object>? context = null);

    Task<List<ConversationLogDto>> GetConversationHistoryAsync(Guid sessionId);

    Task<List<ConversationLogDto>> GetPatientConversationsAsync(Guid patientId);
}
