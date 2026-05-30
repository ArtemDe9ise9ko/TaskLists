using MongoDB.Driver;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Domain.Shares;
using TaskLists.Infrastructure.Persistence.MongoDb.Mapping;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Repositories;

public sealed class MongoTaskListShareRepository(
    MongoDbContext context) : ITaskListShareRepository
{
    public Task<bool> ExistsAsync(
        string taskListId,
        string userId,
        CancellationToken cancellationToken)
    {
        return context.TaskListShares
            .Find(share => share.TaskListId == taskListId && share.UserId == userId)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskListShare>> GetByTaskListIdAsync(
        string taskListId,
        CancellationToken cancellationToken)
    {
        var documents = await context.TaskListShares
            .Find(share => share.TaskListId == taskListId)
            .SortBy(share => share.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(document => document.ToDomain()).ToList();
    }

    public Task AddAsync(
        TaskListShare share,
        CancellationToken cancellationToken)
    {
        return context.TaskListShares.InsertOneAsync(
            share.ToDocument(),
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        string taskListId,
        string userId,
        CancellationToken cancellationToken)
    {
        return context.TaskListShares.DeleteOneAsync(
            share => share.TaskListId == taskListId && share.UserId == userId,
            cancellationToken);
    }

    public Task DeleteByTaskListIdAsync(
        string taskListId,
        CancellationToken cancellationToken)
    {
        return context.TaskListShares.DeleteManyAsync(
            share => share.TaskListId == taskListId,
            cancellationToken);
    }
}
