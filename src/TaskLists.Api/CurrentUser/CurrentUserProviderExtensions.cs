using TaskLists.Application.Abstractions.CurrentUser;
using TaskLists.Application.Exceptions;

namespace TaskLists.Api.CurrentUser;

public static class CurrentUserProviderExtensions
{
    public static string GetRequiredUserId(this ICurrentUserProvider currentUserProvider)
    {
        return currentUserProvider.UserId
            ?? throw new ValidationException(
                "The X-User-Id header is required and must contain one valid value.");
    }
}
