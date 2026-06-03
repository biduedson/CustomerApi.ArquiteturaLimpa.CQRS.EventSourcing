using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services
            .AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.ColorScheme = ColorScheme.Dark;
                options.EnableServerTimingHeader = true;
                options.TrackConnectionOpenClose = true;
                options.EnableDebugMode = environment.IsDevelopment();
            }).AddEntityFramework();

        return services;
    }
}
