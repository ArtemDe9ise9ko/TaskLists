using TaskLists.Domain.TaskLists;

namespace TaskLists.Application.AccessControl;

public sealed class TaskListAccessPolicy : ITaskListAccessPolicy
{
    public bool CanRead(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId) || isShared;
    }

    public bool CanUpdate(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId) || isShared;
    }

    public bool CanDelete(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId);
    }

    public bool CanViewShares(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId) || isShared;
    }

    public bool CanAddShare(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId);
    }

    public bool CanRemoveShare(TaskList taskList, string currentUserId, bool isShared)
    {
        return IsOwner(taskList, currentUserId);
    }

    private static bool IsOwner(TaskList taskList, string currentUserId)
    {
        return taskList.OwnerUserId == currentUserId;
    }
}
