using TaskLists.Domain.Shares;

namespace TaskLists.Application.Abstractions.Persistence;

public interface ITaskListShareRepository
{
    Task<bool> ExistsAsync(
        string taskListId,
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskListShare>> GetByTaskListIdAsync(
        string taskListId,
        CancellationToken cancellationToken);

    Task AddAsync(
        TaskListShare share,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string taskListId,
        string userId,
        CancellationToken cancellationToken);

    Task DeleteByTaskListIdAsync(
        string taskListId,
        CancellationToken cancellationToken);
}
