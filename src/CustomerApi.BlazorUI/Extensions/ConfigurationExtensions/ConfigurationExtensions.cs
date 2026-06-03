using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Extensions.ConfigurationExtensions;

public static class ConfigurationExtensions
{
    public static TOptions? GetOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, IAppOptions
    {
        return configuration
            .GetRequiredSection(TOptions.ConfigSectionPath)
            .Get<TOptions>(options => options.BindNonPublicProperties = true);
    }
}
