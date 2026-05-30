using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Infrastructure.Options;
using TaskLists.Infrastructure.Persistence.MongoDb;
using TaskLists.Infrastructure.Persistence.MongoDb.Indexes;
using TaskLists.Infrastructure.Persistence.MongoDb.Repositories;

namespace TaskLists.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        var options = configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{MongoDbOptions.SectionName}' is required.");

        if (string.IsNullOrWhiteSpace(options.ConnectionString) ||
            string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            throw new InvalidOperationException(
                "MongoDb ConnectionString and DatabaseName are required.");
        }

        services.AddSingleton(options);
        services.AddSingleton<IMongoClient>(_ => new MongoClient(options.ConnectionString));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<ITaskListRepository, MongoTaskListRepository>();
        services.AddSingleton<ITaskListShareRepository, MongoTaskListShareRepository>();
        services.AddHostedService<MongoDbIndexInitializer>();

        return services;
    }
}
