using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Domain.Entities;

namespace HospitalChatbot.Infrastructure.Services;

public class SessionService : ISessionService
{
    private static readonly Dictionary<Guid, ChatSession> _sessions = new();

    public Task<ChatSession> CreateSessionAsync()
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            State = ConversationState.Initial,
            Context = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _sessions[session.Id] = session;
        return Task.FromResult(session);
    }

    public Task<ChatSession?> GetSessionAsync(Guid sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task UpdateSessionAsync(ChatSession session)
    {
        session.LastUpdatedAt = DateTime.UtcNow;
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }
}
