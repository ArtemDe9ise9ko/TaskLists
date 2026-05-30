using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TaskLists.Infrastructure.Persistence.MongoDb.Documents;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Indexes;

public sealed class MongoDbIndexInitializer(MongoDbContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateTaskListIndexesAsync(cancellationToken);
        await CreateTaskListShareIndexesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task CreateTaskListIndexesAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            new CreateIndexModel<TaskListDocument>(
                Builders<TaskListDocument>.IndexKeys
                    .Ascending(taskList => taskList.OwnerUserId),
                new CreateIndexOptions { Name = "ownerUserId" }),
            new CreateIndexModel<TaskListDocument>(
                Builders<TaskListDocument>.IndexKeys
                    .Descending(taskList => taskList.CreatedAtUtc),
                new CreateIndexOptions { Name = "createdAtUtc_desc" }),
            new CreateIndexModel<TaskListDocument>(
                Builders<TaskListDocument>.IndexKeys
                    .Descending(taskList => taskList.CreatedAtUtc)
                    .Descending(taskList => taskList.Id),
                new CreateIndexOptions { Name = "createdAtUtc_desc_id_desc" })
        };

        return context.TaskLists.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    private Task CreateTaskListShareIndexesAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            new CreateIndexModel<TaskListShareDocument>(
                Builders<TaskListShareDocument>.IndexKeys
                    .Ascending(share => share.TaskListId),
                new CreateIndexOptions { Name = "taskListId" }),
            new CreateIndexModel<TaskListShareDocument>(
                Builders<TaskListShareDocument>.IndexKeys
                    .Ascending(share => share.UserId),
                new CreateIndexOptions { Name = "userId" }),
            new CreateIndexModel<TaskListShareDocument>(
                Builders<TaskListShareDocument>.IndexKeys
                    .Ascending(share => share.TaskListId)
                    .Ascending(share => share.UserId),
                new CreateIndexOptions
                {
                    Name = "taskListId_userId_unique",
                    Unique = true
                })
        };

        return context.TaskListShares.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
