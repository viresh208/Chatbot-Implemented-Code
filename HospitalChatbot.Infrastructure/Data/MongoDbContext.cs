using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using HospitalChatbot.Infrastructure.Models;

namespace HospitalChatbot.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is not configured");
        }

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentNullException(nameof(databaseName), "MongoDB database name is not configured");
        }

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<AppointmentLog> AppointmentLogs =>
        _database.GetCollection<AppointmentLog>("AppointmentLogs");

    public IMongoCollection<ConversationLog> ConversationLogs =>
        _database.GetCollection<ConversationLog>("ConversationLogs");
}
