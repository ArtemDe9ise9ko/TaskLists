using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Application.Abstractions.Time;
using TaskLists.Application.AccessControl;
using TaskLists.Application.Exceptions;
using TaskLists.Contracts.Common;
using TaskLists.Contracts.Shares;
using TaskLists.Contracts.TaskLists;
using TaskLists.Domain.Shares;
using TaskLists.Domain.TaskLists;

namespace TaskLists.Application.TaskLists;

public sealed class TaskListService(
    ITaskListRepository taskListRepository,
    ITaskListShareRepository taskListShareRepository,
    ITaskListAccessPolicy accessPolicy,
    IClock clock) : ITaskListService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<TaskListDetailsResponse> CreateAsync(
        CreateTaskListRequest request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var taskList = CreateTaskList(request.Title, currentUserId);

        await taskListRepository.AddAsync(taskList, cancellationToken);

        return MapDetails(taskList);
    }

    public async Task<TaskListDetailsResponse> UpdateAsync(
        string id,
        UpdateTaskListRequest request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var taskList = await GetRequiredTaskListAsync(id, cancellationToken);
        var isShared = await IsSharedAsync(taskList, currentUserId, cancellationToken);

        if (!accessPolicy.CanUpdate(taskList, currentUserId, isShared))
        {
            throw new ForbiddenException("The current user cannot update this task list.");
        }

        TryValidate(() => taskList.UpdateTitle(request.Title, clock.UtcNow));

        await taskListRepository.UpdateAsync(taskList, cancellationToken);

        return MapDetails(taskList);
    }

    public async Task DeleteAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        currentUserId = RequireValue(currentUserId, nameof(currentUserId));
        var taskList = await GetRequiredTaskListAsync(id, cancellationToken);

        if (!accessPolicy.CanDelete(taskList, currentUserId, isShared: false))
        {
            throw new ForbiddenException("Only the owner can delete this task list.");
        }

        await taskListRepository.DeleteAsync(taskList.Id, cancellationToken);
        await taskListShareRepository.DeleteByTaskListIdAsync(
            taskList.Id,
            cancellationToken);
    }

    public async Task<TaskListDetailsResponse> GetByIdAsync(
        string id,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var taskList = await GetRequiredTaskListAsync(id, cancellationToken);
        var isShared = await IsSharedAsync(taskList, currentUserId, cancellationToken);

        if (!accessPolicy.CanRead(taskList, currentUserId, isShared))
        {
            throw new ForbiddenException("The current user cannot access this task list.");
        }

        return MapDetails(taskList);
    }

    public async Task<PagedResponse<TaskListSummaryResponse>> GetPageAsync(
        string currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        currentUserId = RequireValue(currentUserId, nameof(currentUserId));
        page = page < DefaultPage ? DefaultPage : page;
        pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var taskLists = await taskListRepository.GetPageForUserAsync(
            currentUserId,
            page,
            pageSize,
            cancellationToken);
        var totalCount = await taskListRepository.CountForUserAsync(
            currentUserId,
            cancellationToken);

        return new PagedResponse<TaskListSummaryResponse>(
            taskLists.Select(MapSummary).ToList(),
            page,
            pageSize,
            totalCount);
    }

    public async Task<TaskListShareResponse> AddShareAsync(
        string taskListId,
        AddTaskListShareRequest request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        currentUserId = RequireValue(currentUserId, nameof(currentUserId));
        var taskList = await GetRequiredTaskListAsync(taskListId, cancellationToken);

        if (!accessPolicy.CanAddShare(taskList, currentUserId, isShared: false))
        {
            throw new ForbiddenException("Only the owner can add task list shares.");
        }

        var userId = RequireValue(request.UserId, nameof(request.UserId));

        if (taskList.OwnerUserId == userId)
        {
            throw new ConflictException("The owner cannot be added as a shared user.");
        }

        if (await taskListShareRepository.ExistsAsync(taskList.Id, userId, cancellationToken))
        {
            throw new ConflictException("The task list is already shared with this user.");
        }

        var share = CreateShare(taskList.Id, userId);

        await taskListShareRepository.AddAsync(share, cancellationToken);

        return MapShare(share);
    }

    public async Task<IReadOnlyList<TaskListShareResponse>> GetSharesAsync(
        string taskListId,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        currentUserId = RequireValue(currentUserId, nameof(currentUserId));
        var taskList = await GetRequiredTaskListAsync(taskListId, cancellationToken);
        var isShared = await IsSharedAsync(taskList, currentUserId, cancellationToken);

        if (!accessPolicy.CanViewShares(taskList, currentUserId, isShared))
        {
            throw new ForbiddenException("The current user cannot view task list shares.");
        }

        var shares = await taskListShareRepository.GetByTaskListIdAsync(
            taskList.Id,
            cancellationToken);

        return shares.Select(MapShare).ToList();
    }

    public async Task RemoveShareAsync(
        string taskListId,
        string userId,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        currentUserId = RequireValue(currentUserId, nameof(currentUserId));
        var taskList = await GetRequiredTaskListAsync(taskListId, cancellationToken);

        if (!accessPolicy.CanRemoveShare(taskList, currentUserId, isShared: false))
        {
            throw new ForbiddenException("Only the owner can remove task list shares.");
        }

        userId = RequireValue(userId, nameof(userId));

        await taskListShareRepository.DeleteAsync(taskList.Id, userId, cancellationToken);
    }

    private async Task<TaskList> GetRequiredTaskListAsync(
        string id,
        CancellationToken cancellationToken)
    {
        id = RequireValue(id, nameof(id));

        return await taskListRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Task list was not found.");
    }

    private async Task<bool> IsSharedAsync(
        TaskList taskList,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        currentUserId = RequireValue(currentUserId, nameof(currentUserId));

        return taskList.OwnerUserId != currentUserId &&
            await taskListShareRepository.ExistsAsync(
                taskList.Id,
                currentUserId,
                cancellationToken);
    }

    private TaskList CreateTaskList(string title, string ownerUserId)
    {
        var utcNow = clock.UtcNow;

        return TryValidate(() => new TaskList(
            Guid.NewGuid().ToString(),
            title,
            ownerUserId,
            utcNow,
            utcNow));
    }

    private TaskListShare CreateShare(string taskListId, string userId)
    {
        return TryValidate(() => new TaskListShare(
            Guid.NewGuid().ToString(),
            taskListId,
            userId,
            clock.UtcNow));
    }

    private static TaskListDetailsResponse MapDetails(TaskList taskList)
    {
        return new TaskListDetailsResponse(
            taskList.Id,
            taskList.Title,
            taskList.OwnerUserId,
            taskList.CreatedAtUtc,
            taskList.UpdatedAtUtc);
    }

    private static TaskListSummaryResponse MapSummary(TaskList taskList)
    {
        return new TaskListSummaryResponse(
            taskList.Id,
            taskList.Title,
            taskList.CreatedAtUtc);
    }

    private static TaskListShareResponse MapShare(TaskListShare share)
    {
        return new TaskListShareResponse(share.UserId, share.CreatedAtUtc);
    }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{parameterName} cannot be empty.");
        }

        return value.Trim();
    }

    private static void TryValidate(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }

    private static T TryValidate<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }
}
