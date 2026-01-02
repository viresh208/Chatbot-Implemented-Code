using MongoDB.Driver;
using MongoDB.Bson;
using HospitalChatbot.Application.Interfaces;
using HospitalChatbot.Application.DTOs;
using HospitalChatbot.Infrastructure.Data;
using HospitalChatbot.Infrastructure.Models;

namespace HospitalChatbot.Infrastructure.Services;

public class ConversationLogService : IConversationLogService
{
    private readonly MongoDbContext _mongoContext;

    public ConversationLogService(MongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    public async Task LogConversationAsync(
        Guid sessionId,
        Guid? patientId,
        string? patientName,
        string conversationState,
        string userMessage,
        string botResponse,
        List<OptionDto>? options = null,
        Dictionary<string, object>? context = null)
    {
        var messageCount = await _mongoContext.ConversationLogs
            .CountDocumentsAsync(c => c.SessionId == sessionId);

        var contextBson = context?.ToDictionary(
            kvp => kvp.Key,
            kvp => ConvertToBsonValue(kvp.Value)
        );

        var conversationLog = new ConversationLog
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            PatientId = patientId,
            PatientName = patientName,
            ConversationState = conversationState,
            UserMessage = userMessage,
            BotResponse = botResponse,
            Options = options?.Select(o => new OptionMongo
            {
                Id = o.Id,
                Text = o.Display
            }).ToList(),
            Context = contextBson,
            Timestamp = DateTime.UtcNow,
            MessageNumber = (int)messageCount + 1
        };

        await _mongoContext.ConversationLogs.InsertOneAsync(conversationLog);
    }

    public async Task<List<ConversationLogDto>> GetConversationHistoryAsync(Guid sessionId)
    {
        var logs = await _mongoContext.ConversationLogs
            .Find(c => c.SessionId == sessionId)
            .SortBy(c => c.Timestamp)
            .ToListAsync();

        return logs.Select(log => new ConversationLogDto
        {
            Id = log.Id,
            SessionId = log.SessionId,
            PatientId = log.PatientId,
            PatientName = log.PatientName,
            ConversationState = log.ConversationState,
            UserMessage = log.UserMessage,
            BotResponse = log.BotResponse,
            Options = log.Options?.Select(o => new OptionDto
            {
                Id = o.Id,
                Display = o.Text,
                Value = o.Id
            }).ToList(),
            Context = log.Context?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)(kvp.Value.ToString() ?? string.Empty)
            ),
            Timestamp = log.Timestamp,
            MessageNumber = log.MessageNumber
        }).ToList();
    }

    public async Task<List<ConversationLogDto>> GetPatientConversationsAsync(Guid patientId)
    {
        var logs = await _mongoContext.ConversationLogs
            .Find(c => c.PatientId == patientId)
            .SortBy(c => c.Timestamp)
            .ToListAsync();

        return logs.Select(log => new ConversationLogDto
        {
            Id = log.Id,
            SessionId = log.SessionId,
            PatientId = log.PatientId,
            PatientName = log.PatientName,
            ConversationState = log.ConversationState,
            UserMessage = log.UserMessage,
            BotResponse = log.BotResponse,
            Options = log.Options?.Select(o => new OptionDto
            {
                Id = o.Id,
                Display = o.Text,
                Value = o.Id
            }).ToList(),
            Context = log.Context?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)(kvp.Value.ToString() ?? string.Empty)
            ),
            Timestamp = log.Timestamp,
            MessageNumber = log.MessageNumber
        }).ToList();
    }

    private static BsonValue ConvertToBsonValue(object value)
    {
        if (value == null)
            return BsonNull.Value;

        return value switch
        {
            Guid guidValue => BsonString.Create(guidValue.ToString()),
            string stringValue => BsonString.Create(stringValue),
            int intValue => BsonInt32.Create(intValue),
            long longValue => BsonInt64.Create(longValue),
            double doubleValue => BsonDouble.Create(doubleValue),
            bool boolValue => BsonBoolean.Create(boolValue),
            DateTime dateTimeValue => BsonDateTime.Create(dateTimeValue),
            _ => BsonString.Create(value.ToString() ?? string.Empty)
        };
    }
}
