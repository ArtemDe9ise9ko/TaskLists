using TaskLists.Application.AccessControl;
using TaskLists.Domain.TaskLists;

namespace TaskLists.Tests.Application.AccessControl;

public sealed class TaskListAccessPolicyTests
{
    private const string OwnerUserId = "owner-user";
    private const string SharedUserId = "shared-user";
    private const string UnrelatedUserId = "unrelated-user";

    private readonly TaskListAccessPolicy _policy = new();
    private readonly TaskList _taskList = new(
        "task-list-id",
        "Release checklist",
        OwnerUserId,
        DateTime.UtcNow,
        DateTime.UtcNow);

    [Fact]
    public void Owner_CanPerformEveryOperation()
    {
        Assert.True(_policy.CanRead(_taskList, OwnerUserId, isShared: false));
        Assert.True(_policy.CanUpdate(_taskList, OwnerUserId, isShared: false));
        Assert.True(_policy.CanDelete(_taskList, OwnerUserId, isShared: false));
        Assert.True(_policy.CanViewShares(_taskList, OwnerUserId, isShared: false));
        Assert.True(_policy.CanAddShare(_taskList, OwnerUserId, isShared: false));
        Assert.True(_policy.CanRemoveShare(_taskList, OwnerUserId, isShared: false));
    }

    [Fact]
    public void SharedUser_CanReadUpdateAndViewSharesOnly()
    {
        Assert.True(_policy.CanRead(_taskList, SharedUserId, isShared: true));
        Assert.True(_policy.CanUpdate(_taskList, SharedUserId, isShared: true));
        Assert.True(_policy.CanViewShares(_taskList, SharedUserId, isShared: true));

        Assert.False(_policy.CanDelete(_taskList, SharedUserId, isShared: true));
        Assert.False(_policy.CanAddShare(_taskList, SharedUserId, isShared: true));
        Assert.False(_policy.CanRemoveShare(_taskList, SharedUserId, isShared: true));
    }

    [Fact]
    public void UnrelatedUser_CannotPerformAnyOperation()
    {
        Assert.False(_policy.CanRead(_taskList, UnrelatedUserId, isShared: false));
        Assert.False(_policy.CanUpdate(_taskList, UnrelatedUserId, isShared: false));
        Assert.False(_policy.CanDelete(_taskList, UnrelatedUserId, isShared: false));
        Assert.False(_policy.CanViewShares(_taskList, UnrelatedUserId, isShared: false));
        Assert.False(_policy.CanAddShare(_taskList, UnrelatedUserId, isShared: false));
        Assert.False(_policy.CanRemoveShare(_taskList, UnrelatedUserId, isShared: false));
    }
}
