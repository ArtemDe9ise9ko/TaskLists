namespace TaskLists.Application.Abstractions.CurrentUser;

public interface ICurrentUserProvider
{
    string? UserId { get; }
}
