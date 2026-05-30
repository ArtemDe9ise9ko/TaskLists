namespace TaskLists.Domain.Shares;

public sealed class TaskListShare
{
    public TaskListShare(
        string id,
        string taskListId,
        string userId,
        DateTime createdAtUtc)
    {
        Id = RequireValue(id, nameof(id));
        TaskListId = RequireValue(taskListId, nameof(taskListId));
        UserId = RequireValue(userId, nameof(userId));
        CreatedAtUtc = createdAtUtc;
    }

    public string Id { get; }

    public string TaskListId { get; }

    public string UserId { get; }

    public DateTime CreatedAtUtc { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
