using CustomerApi.Core.AppSettings;
using CustomerApi.Core.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Core.Extensions.ServiceCollectionsExtensions;

public static class AppSettingsExtensions
{
    public static IServiceCollection ConfigureAppSettings(this IServiceCollection services) =>
        services
            .AddOptionsWithValidation<ConnectionOptions>()
            .AddOptionsWithValidation<CacheOptions>()
            .AddOptionsWithValidation<JwtOptions>()
            .AddOptionsWithValidation<AdminSeedOptions>();

    private static IServiceCollection AddOptionsWithValidation<TOptions>(this IServiceCollection services)
        where TOptions : class, IAppOptions
    {
        return services
            .AddOptions<TOptions>()
            .BindConfiguration(TOptions.ConfigSectionPath, binderOptions => binderOptions.BindNonPublicProperties = true)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Services;
    }
}
