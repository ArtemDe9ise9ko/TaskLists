using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Time;
using TaskLists.Application.AccessControl;
using TaskLists.Application.Time;

namespace TaskLists.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITaskListAccessPolicy, TaskListAccessPolicy>();

        return services;
    }
}
