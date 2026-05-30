using TaskLists.Domain.TaskLists;

namespace TaskLists.Tests.Domain.TaskLists;

public sealed class TaskListTests
{
    [Fact]
    public void Constructor_ThrowsWhenTitleIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateTaskList(title: " "));
    }

    [Fact]
    public void Constructor_ThrowsWhenTitleIsLongerThanMaximum()
    {
        var title = new string('a', TaskList.MaxTitleLength + 1);

        Assert.Throws<ArgumentException>(() => CreateTaskList(title: title));
    }

    [Fact]
    public void Constructor_ThrowsWhenOwnerUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => CreateTaskList(ownerUserId: " "));
    }

    [Fact]
    public void UpdateTitle_UpdatesTitleAndTimestamp()
    {
        var taskList = CreateTaskList();
        var updatedAtUtc = new DateTime(2026, 5, 30, 12, 0, 0, DateTimeKind.Utc);

        taskList.UpdateTitle("Updated title", updatedAtUtc);

        Assert.Equal("Updated title", taskList.Title);
        Assert.Equal(updatedAtUtc, taskList.UpdatedAtUtc);
    }

    private static TaskList CreateTaskList(
        string title = "Release checklist",
        string ownerUserId = "owner-user")
    {
        var createdAtUtc = new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc);

        return new TaskList(
            "task-list-id",
            title,
            ownerUserId,
            createdAtUtc,
            createdAtUtc);
    }
}
