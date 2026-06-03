using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class CacheExtensions
{
    private const string RedisInstanceName = "master";
    public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
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
}
