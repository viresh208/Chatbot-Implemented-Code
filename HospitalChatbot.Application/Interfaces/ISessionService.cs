using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Application.Interfaces;

public interface ISessionService
{
    Task<ChatSession> CreateSessionAsync();
    Task<ChatSession?> GetSessionAsync(Guid sessionId);
    Task UpdateSessionAsync(ChatSession session);
}
