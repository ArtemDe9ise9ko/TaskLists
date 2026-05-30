using TaskLists.Application.Abstractions.CurrentUser;

namespace TaskLists.Api.CurrentUser;

public sealed class HttpContextCurrentUserProvider(
    IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public string? UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is null ||
                !httpContext.Request.Headers.TryGetValue("X-User-Id", out var values) ||
                values.Count != 1)
            {
                return null;
            }

            var userId = values[0]?.Trim();

            return string.IsNullOrWhiteSpace(userId) || userId.Length > 200
                ? null
                : userId;
        }
    }
}
