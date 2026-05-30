using TaskLists.Domain.Shares;

namespace TaskLists.Tests.Domain.Shares;

public sealed class TaskListShareTests
{
    [Fact]
    public void Constructor_ThrowsWhenTaskListIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateShare(taskListId: " "));
    }

    [Fact]
    public void Constructor_ThrowsWhenUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateShare(userId: " "));
    }

    private static TaskListShare CreateShare(
        string taskListId = "task-list-id",
        string userId = "shared-user")
    {
        return new TaskListShare(
            "share-id",
            taskListId,
            userId,
            new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc));
    }
}
