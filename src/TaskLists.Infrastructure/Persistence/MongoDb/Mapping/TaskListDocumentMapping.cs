using TaskLists.Domain.TaskLists;
using TaskLists.Infrastructure.Persistence.MongoDb.Documents;

namespace TaskLists.Infrastructure.Persistence.MongoDb.Mapping;

internal static class TaskListDocumentMapping
{
    public static TaskList ToDomain(this TaskListDocument document)
    {
        return new TaskList(
            document.Id,
            document.Title,
            document.OwnerUserId,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
    }

    public static TaskListDocument ToDocument(this TaskList taskList)
    {
        return new TaskListDocument
        {
            Id = taskList.Id,
            Title = taskList.Title,
            OwnerUserId = taskList.OwnerUserId,
            CreatedAtUtc = taskList.CreatedAtUtc,
            UpdatedAtUtc = taskList.UpdatedAtUtc
        };
    }
}
