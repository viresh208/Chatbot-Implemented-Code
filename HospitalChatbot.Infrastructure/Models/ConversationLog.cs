using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalChatbot.Infrastructure.Models;

public class ConversationLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("sessionId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SessionId { get; set; }

    [BsonElement("patientId")]
    [BsonRepresentation(BsonType.String)]
    public Guid? PatientId { get; set; }

    [BsonElement("patientName")]
    public string? PatientName { get; set; }

    [BsonElement("conversationState")]
    public string ConversationState { get; set; } = string.Empty;

    [BsonElement("userMessage")]
    public string UserMessage { get; set; } = string.Empty;

    [BsonElement("botResponse")]
    public string BotResponse { get; set; } = string.Empty;

    [BsonElement("options")]
    public List<OptionMongo>? Options { get; set; }

    [BsonElement("context")]
    public Dictionary<string, BsonValue>? Context { get; set; }

    [BsonElement("timestamp")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Timestamp { get; set; }

    [BsonElement("messageNumber")]
    public int MessageNumber { get; set; }
}

public class OptionMongo
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;
}
