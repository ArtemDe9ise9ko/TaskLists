using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Application.Abstractions.Time;
using TaskLists.Application.AccessControl;
using TaskLists.Application.Exceptions;
using TaskLists.Application.TaskLists;
using TaskLists.Contracts.Shares;
using TaskLists.Contracts.TaskLists;
using TaskLists.Domain.Shares;
using TaskLists.Domain.TaskLists;

namespace TaskLists.Tests.Application.TaskLists;

public sealed class TaskListServiceTests
{
    private static readonly DateTime UtcNow =
        new(2026, 5, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_SetsOwnerAndTimestamps()
    {
        var context = CreateContext();

        var response = await context.Service.CreateAsync(
            new CreateTaskListRequest("Release checklist"),
            "owner-user",
            CancellationToken.None);

        var taskList = Assert.Single(context.TaskLists.Items.Values);
        Assert.Equal("owner-user", taskList.OwnerUserId);
        Assert.Equal(UtcNow, taskList.CreatedAtUtc);
        Assert.Equal(UtcNow, taskList.UpdatedAtUtc);
        Assert.Equal(taskList.Id, response.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenTaskListDoesNotExist()
    {
        var context = CreateContext();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.Service.GetByIdAsync(
                "missing-id",
                "owner-user",
                CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsForUnrelatedUser()
    {
        var context = CreateContext(CreateTaskList());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            context.Service.GetByIdAsync(
                "task-list-id",
                "unrelated-user",
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_AllowsSharedUser()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        var response = await context.Service.UpdateAsync(
            "task-list-id",
            new UpdateTaskListRequest("Updated title"),
            "shared-user",
            CancellationToken.None);

        Assert.Equal("Updated title", response.Title);
        Assert.Equal(UtcNow, response.UpdatedAtUtc);
        Assert.Equal("Updated title", context.TaskLists.Items["task-list-id"].Title);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsForSharedUser()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            context.Service.DeleteAsync(
                "task-list-id",
                "shared-user",
                CancellationToken.None));

        Assert.Contains("task-list-id", context.TaskLists.Items.Keys);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTaskListShares()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        await context.Service.DeleteAsync(
            "task-list-id",
            "owner-user",
            CancellationToken.None);

        Assert.DoesNotContain("task-list-id", context.TaskLists.Items.Keys);
        Assert.False(context.Shares.Exists("task-list-id", "shared-user"));
    }

    [Fact]
    public async Task AddShareAsync_AllowsOwner()
    {
        var context = CreateContext(CreateTaskList());

        var response = await context.Service.AddShareAsync(
            "task-list-id",
            new AddTaskListShareRequest("shared-user"),
            "owner-user",
            CancellationToken.None);

        Assert.Equal("shared-user", response.UserId);
        Assert.Equal(UtcNow, response.CreatedAtUtc);
        Assert.True(context.Shares.Exists("task-list-id", "shared-user"));
    }

    [Fact]
    public async Task AddShareAsync_ThrowsForSharedUser()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            context.Service.AddShareAsync(
                "task-list-id",
                new AddTaskListShareRequest("another-user"),
                "shared-user",
                CancellationToken.None));
    }

    [Fact]
    public async Task AddShareAsync_ThrowsWhenRelationAlreadyExists()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            context.Service.AddShareAsync(
                "task-list-id",
                new AddTaskListShareRequest("shared-user"),
                "owner-user",
                CancellationToken.None));
    }

    [Fact]
    public async Task AddShareAsync_ThrowsWhenSharingWithOwner()
    {
        var context = CreateContext(CreateTaskList());

        await Assert.ThrowsAsync<ConflictException>(() =>
            context.Service.AddShareAsync(
                "task-list-id",
                new AddTaskListShareRequest("owner-user"),
                "owner-user",
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveShareAsync_ThrowsForSharedUser()
    {
        var context = CreateContext(CreateTaskList());
        context.Shares.Add(CreateShare(userId: "shared-user"));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            context.Service.RemoveShareAsync(
                "task-list-id",
                "another-user",
                "shared-user",
                CancellationToken.None));
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-1, 101, 1, 100)]
    public async Task GetPageAsync_NormalizesInvalidPagination(
        int page,
        int pageSize,
        int expectedPage,
        int expectedPageSize)
    {
        var context = CreateContext();

        var response = await context.Service.GetPageAsync(
            "current-user",
            page,
            pageSize,
            CancellationToken.None);

        Assert.Equal(expectedPage, response.Page);
        Assert.Equal(expectedPageSize, response.PageSize);
        Assert.Equal(expectedPage, context.TaskLists.LastRequestedPage);
        Assert.Equal(expectedPageSize, context.TaskLists.LastRequestedPageSize);
    }

    private static TestContext CreateContext(params TaskList[] taskLists)
    {
        var taskListRepository = new InMemoryTaskListRepository(taskLists);
        var shareRepository = new InMemoryTaskListShareRepository();
        var service = new TaskListService(
            taskListRepository,
            shareRepository,
            new TaskListAccessPolicy(),
            new FixedClock(UtcNow));

        return new TestContext(service, taskListRepository, shareRepository);
    }

    private static TaskList CreateTaskList()
    {
        var createdAtUtc = new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc);

        return new TaskList(
            "task-list-id",
            "Release checklist",
            "owner-user",
            createdAtUtc,
            createdAtUtc);
    }

    private static TaskListShare CreateShare(string userId)
    {
        return new TaskListShare(
            $"share-{userId}",
            "task-list-id",
            userId,
            UtcNow);
    }

    private sealed record TestContext(
        TaskListService Service,
        InMemoryTaskListRepository TaskLists,
        InMemoryTaskListShareRepository Shares);

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;
    }

    private sealed class InMemoryTaskListRepository(
        IEnumerable<TaskList> taskLists) : ITaskListRepository
    {
        public Dictionary<string, TaskList> Items { get; } =
            taskLists.ToDictionary(taskList => taskList.Id);

        public int? LastRequestedPage { get; private set; }

        public int? LastRequestedPageSize { get; private set; }

        public Task<TaskList?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken)
        {
            Items.TryGetValue(id, out var taskList);
            return Task.FromResult(taskList);
        }

        public Task<IReadOnlyList<TaskList>> GetPageForUserAsync(
            string userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            LastRequestedPage = page;
            LastRequestedPageSize = pageSize;

            return Task.FromResult<IReadOnlyList<TaskList>>(
                Items.Values.Take(pageSize).ToList());
        }

        public Task<long> CountForUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult((long)Items.Count);
        }

        public Task AddAsync(
            TaskList taskList,
            CancellationToken cancellationToken)
        {
            Items.Add(taskList.Id, taskList);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            TaskList taskList,
            CancellationToken cancellationToken)
        {
            Items[taskList.Id] = taskList;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            string id,
            CancellationToken cancellationToken)
        {
            Items.Remove(id);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryTaskListShareRepository : ITaskListShareRepository
    {
        private readonly List<TaskListShare> _shares = [];

        public bool Exists(string taskListId, string userId)
        {
            return _shares.Any(share =>
                share.TaskListId == taskListId && share.UserId == userId);
        }

        public void Add(TaskListShare share)
        {
            _shares.Add(share);
        }

        public Task<bool> ExistsAsync(
            string taskListId,
            string userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Exists(taskListId, userId));
        }

        public Task<IReadOnlyList<TaskListShare>> GetByTaskListIdAsync(
            string taskListId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TaskListShare>>(
                _shares.Where(share => share.TaskListId == taskListId).ToList());
        }

        public Task AddAsync(
            TaskListShare share,
            CancellationToken cancellationToken)
        {
            Add(share);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            string taskListId,
            string userId,
            CancellationToken cancellationToken)
        {
            _shares.RemoveAll(share =>
                share.TaskListId == taskListId && share.UserId == userId);

            return Task.CompletedTask;
        }

        public Task DeleteByTaskListIdAsync(
            string taskListId,
            CancellationToken cancellationToken)
        {
            _shares.RemoveAll(share => share.TaskListId == taskListId);
            return Task.CompletedTask;
        }
    }
}
