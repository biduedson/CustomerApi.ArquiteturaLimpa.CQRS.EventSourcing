using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure.Extensions.ServiceCollectionsExtensions;

public static class CacheExtensions
{
    public static void AddMemoryCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, MemoryCacheService>();

    public static void AddDistributedCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, DistributedCacheService>();
}
