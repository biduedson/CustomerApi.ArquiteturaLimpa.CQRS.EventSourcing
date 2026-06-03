using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class HealthChecksExtensions
{
    private static readonly string[] DbRelationalTags = ["database", "ef-core", "sqlserver", "realtional"];
    private static readonly string[] DbNoSqlTags = ["database", "mongodb", "no-sql"];
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetOptions<ConnectionOptions>();

        var hralthCheckBuilder = services
            .AddHealthChecks()
            .AddDbContextCheck<WriteDbContext>(tags: DbRelationalTags)
            .AddDbContextCheck<EventStoreDbContext>(tags: DbRelationalTags)
            .AddMongoDb(clientFactory: _ => new MongoClient(options!.NoSqlConnection), tags: DbNoSqlTags);

        if (!options!.CacheConnectionInMemory())
        {
            hralthCheckBuilder.AddRedis(options.CacheConnection!);
        }

        return services;
    }
}
