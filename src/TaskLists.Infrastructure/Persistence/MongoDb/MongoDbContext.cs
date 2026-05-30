using MongoDB.Driver;
using TaskLists.Infrastructure.Options;
using TaskLists.Infrastructure.Persistence.MongoDb.Documents;

namespace TaskLists.Infrastructure.Persistence.MongoDb;

public sealed class MongoDbContext
{
    public const string TaskListsCollectionName = "taskLists";
    public const string TaskListSharesCollectionName = "taskListShares";

    public MongoDbContext(IMongoClient client, MongoDbOptions options)
    {
        Database = client.GetDatabase(options.DatabaseName);
        TaskLists = Database.GetCollection<TaskListDocument>(TaskListsCollectionName);
        TaskListShares = Database.GetCollection<TaskListShareDocument>(
            TaskListSharesCollectionName);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<TaskListDocument> TaskLists { get; }

    public IMongoCollection<TaskListShareDocument> TaskListShares { get; }
}
