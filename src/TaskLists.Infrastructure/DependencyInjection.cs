using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskLists.Infrastructure.Options;

namespace TaskLists.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        return services;
    }
}
