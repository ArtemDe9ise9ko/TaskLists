using TaskLists.Domain.TaskLists;

namespace TaskLists.Application.Abstractions.Persistence;

public interface ITaskListRepository
{
    Task<TaskList?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskList>> GetPageForUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<long> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        TaskList taskList,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        TaskList taskList,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken);
}
