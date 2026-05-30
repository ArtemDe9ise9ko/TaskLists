using TaskLists.Domain.TaskLists;

namespace TaskLists.Application.AccessControl;

public interface ITaskListAccessPolicy
{
    bool CanRead(TaskList taskList, string currentUserId, bool isShared);

    bool CanUpdate(TaskList taskList, string currentUserId, bool isShared);

    bool CanDelete(TaskList taskList, string currentUserId, bool isShared);

    bool CanViewShares(TaskList taskList, string currentUserId, bool isShared);

    bool CanAddShare(TaskList taskList, string currentUserId, bool isShared);

    bool CanRemoveShare(TaskList taskList, string currentUserId, bool isShared);
}
