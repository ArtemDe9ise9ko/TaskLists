namespace TaskLists.Contracts.TaskLists;

public sealed record TaskListSummaryResponse(
    string Id,
    string Title,
    DateTime CreatedAtUtc);
