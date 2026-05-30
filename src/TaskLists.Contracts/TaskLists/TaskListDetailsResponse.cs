namespace TaskLists.Contracts.TaskLists;

public sealed record TaskListDetailsResponse(
    string Id,
    string Title,
    string OwnerUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
