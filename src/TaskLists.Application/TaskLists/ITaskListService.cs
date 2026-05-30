using TaskLists.Contracts.Common;
using TaskLists.Contracts.Shares;
using TaskLists.Contracts.TaskLists;

namespace TaskLists.Application.TaskLists;

public interface ITaskListService
{
    Task<TaskListDetailsResponse> CreateAsync(
        CreateTaskListRequest request,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<TaskListDetailsResponse> UpdateAsync(
        string id,
        UpdateTaskListRequest request,
        string currentUserId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<TaskListDetailsResponse> GetByIdAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<PagedResponse<TaskListSummaryResponse>> GetPageAsync(
        string currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<TaskListShareResponse> AddShareAsync(
        string taskListId,
        AddTaskListShareRequest request,
        string currentUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskListShareResponse>> GetSharesAsync(
        string taskListId,
        string currentUserId,
        CancellationToken cancellationToken);

    Task RemoveShareAsync(
        string taskListId,
        string userId,
        string currentUserId,
        CancellationToken cancellationToken);
}
