using TaskLists.Domain.Shares;
using TaskLists.Infrastructure.Persistence.MongoDb.Documents;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Mapping;

internal static class TaskListShareDocumentMapping
{
    public static TaskListShare ToDomain(this TaskListShareDocument document)
    {
        return new TaskListShare(
            document.Id,
            document.TaskListId,
            document.UserId,
            document.CreatedAtUtc);
    }

    public static TaskListShareDocument ToDocument(this TaskListShare share)
    {
        return new TaskListShareDocument
        {
            Id = share.Id,
            TaskListId = share.TaskListId,
            UserId = share.UserId,
            CreatedAtUtc = share.CreatedAtUtc
        };
    }
}
