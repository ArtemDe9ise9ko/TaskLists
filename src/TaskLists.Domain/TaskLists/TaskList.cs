namespace TaskLists.Domain.TaskLists;

public sealed class TaskList
{
    public const int MaxTitleLength = 255;

    public TaskList(
        string id,
        string title,
        string ownerUserId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = RequireValue(id, nameof(id));
        Title = ValidateTitle(title);
        OwnerUserId = RequireValue(ownerUserId, nameof(ownerUserId));
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public string Id { get; }

    public string Title { get; private set; }

    public string OwnerUserId { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void UpdateTitle(string title, DateTime updatedAtUtc)
    {
        Title = ValidateTitle(title);
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string ValidateTitle(string title)
    {
        var normalizedTitle = RequireValue(title, nameof(title));

        if (normalizedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Title must not exceed {MaxTitleLength} characters.",
                nameof(title));
        }

        return normalizedTitle;
    }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
