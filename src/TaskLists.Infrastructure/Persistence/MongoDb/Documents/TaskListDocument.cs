using MongoDB.Bson.Serialization.Attributes;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Documents;

public sealed class TaskListDocument
{
    [BsonId]
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string OwnerUserId { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
