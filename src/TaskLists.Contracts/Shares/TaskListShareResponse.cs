namespace TaskLists.Contracts.Shares;

public sealed record TaskListShareResponse(
    string UserId,
    DateTime CreatedAtUtc);
