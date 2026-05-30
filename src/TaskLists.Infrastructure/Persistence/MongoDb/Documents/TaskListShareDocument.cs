using MongoDB.Bson.Serialization.Attributes;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Documents;

public sealed class TaskListShareDocument
{
    [BsonId]
    public required string Id { get; init; }

    public required string TaskListId { get; init; }

    public required string UserId { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
