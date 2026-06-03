using System;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class DatabaseExtensions
{
    private const int DbMaxRetryCount = 3;
    private const int DBCommandTimeout = 30;
    private const string DbMigrationAssemblyName = "CustomerApi.WebApi";
    private const string TestingEnvironmentName = "Testing";



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
