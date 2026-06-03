using CustomerApi.BlazorUI.Abstractions;
using CustomerApi.BlazorUI.Settings;

namespace CustomerApi.BlazorUI.Extensions.ServiceCollectionsExtensions;

public static class AppSettingsExtensions
{
    public static IServiceCollection ConfigureAppSettings(this IServiceCollection services) =>
        services.AddOptionsWithValidation<CustomerApiSettings>();

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
