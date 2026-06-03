using System;
using System.Diagnostics.CodeAnalysis;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Infrastructure;
using CustomerApi.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CustomerApi.WebApi.Extensions;

[ExcludeFromCodeCoverage]
internal static class ServicesCollectionExtensions
{
    private const int DbMaxRetryCount = 3;
    private const int DBCommandTimeout = 30;
    private const string DbMigrationAssemblyName = "CustomerApi.WebApi";
    private const string RedisInstanceName = "master";
    private const string TestingEnvironmentName = "Testing";

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
    public static IServiceCollection AddWriteDbContext(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (!environment.IsEnvironment(TestingEnvironmentName))
        {
            services.AddDbContextPool<WriteDbContext>((serviceProvider, optionsBuilder) =>
                ConfigureDbContext<WriteDbContext>(
                    serviceProvider, optionsBuilder, QueryTrackingBehavior.TrackAll));

            services.AddDbContextPool<EventStoreDbContext>((serviceProvider, optionsBuilder) =>
                ConfigureDbContext<EventStoreDbContext>(
                    serviceProvider, optionsBuilder, QueryTrackingBehavior.NoTrackingWithIdentityResolution));
        }

        return services;
    }
    public static IServiceCollection AddCAcheService(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetOptions<ConnectionOptions>();
        if (options!.CacheConnectionInMemory())
        {
            services.AddMemoryCacheService();
            services.AddMemoryCache(memoryOptions => memoryOptions.TrackStatistics = true);
        }
        else
        {
            services.AddDistributedCacheService();
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.InstanceName = RedisInstanceName;
                redisOptions.Configuration = options.CacheConnection;
            });
        }
        return services;
    }
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddJwtAuthentication(configuration)
                .AddAuthorizationPolicies();

    private static void ConfigureDbContext<TDbcontext>(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder optionsBuilder,
        QueryTrackingBehavior queryTrackingBehavior) where TDbcontext : DbContext
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TDbcontext>>();
        var options = serviceProvider.GetOptions<ConnectionOptions>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>(); ;
        var envIsDevelopment = environment.IsDevelopment();

        optionsBuilder
        .UseSqlServer(options.SqlConnection, sqlServerOptions =>
        {
            sqlServerOptions
            .MigrationsAssembly(DbMigrationAssemblyName)
            .EnableRetryOnFailure(DbMaxRetryCount)
            .CommandTimeout(DBCommandTimeout);
        })
        .EnableDetailedErrors(envIsDevelopment)
        .EnableSensitiveDataLogging(envIsDevelopment)
        .UseQueryTrackingBehavior(queryTrackingBehavior)
        .LogTo((eventId, _) => eventId.Id == CoreEventId.ExecutionStrategyRetrying, eventData =>
        {
            if (eventData is not ExecutionStrategyEventData retryEventData)
                return;
            var exceptions = retryEventData.ExceptionsEncountered;

            logger.LogWarning(
                "----- DbContext: Tentativa #{Count} com atraso de {Delay} devido ao erro: {Message}",
                 exceptions.Count,
                 retryEventData.Delay,
                 exceptions[^1].Message);
        });

        if (envIsDevelopment)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }


}