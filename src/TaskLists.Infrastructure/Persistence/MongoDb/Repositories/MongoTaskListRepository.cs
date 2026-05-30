using MongoDB.Driver;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Domain.TaskLists;
using TaskLists.Infrastructure.Persistence.MongoDb.Documents;
using TaskLists.Infrastructure.Persistence.MongoDb.Mapping;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Repositories;

public sealed class MongoTaskListRepository(MongoDbContext context) : ITaskListRepository
{
    public async Task<TaskList?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var document = await context.TaskLists
            .Find(taskList => taskList.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<TaskList>> GetPageForUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = await BuildAccessibleFilterAsync(userId, cancellationToken);
        var skip = checked((page - 1) * pageSize);

        var documents = await context.TaskLists
            .Find(filter)
            .SortByDescending(taskList => taskList.CreatedAtUtc)
            .ThenByDescending(taskList => taskList.Id)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return documents.Select(document => document.ToDomain()).ToList();
    }

    public async Task<long> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var filter = await BuildAccessibleFilterAsync(userId, cancellationToken);

        return await context.TaskLists.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
    }

    public Task AddAsync(
        TaskList taskList,
        CancellationToken cancellationToken)
    {
        return context.TaskLists.InsertOneAsync(
            taskList.ToDocument(),
            cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(
        TaskList taskList,
        CancellationToken cancellationToken)
    {
        return context.TaskLists.ReplaceOneAsync(
            document => document.Id == taskList.Id,
            taskList.ToDocument(),
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return context.TaskLists.DeleteOneAsync(
            taskList => taskList.Id == id,
            cancellationToken);
    }

    private async Task<FilterDefinition<TaskListDocument>> BuildAccessibleFilterAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var sharedTaskListIds = await context.TaskListShares
            .Find(share => share.UserId == userId)
            .Project(share => share.TaskListId)
            .ToListAsync(cancellationToken);

        var filters = Builders<TaskListDocument>.Filter;

        return filters.Or(
            filters.Eq(taskList => taskList.OwnerUserId, userId),
            filters.In(taskList => taskList.Id, sharedTaskListIds));
    }
}
