using CustomerApi.BlazorUI.Services;
using CustomerApi.BlazorUI.Settings;

namespace CustomerApi.BlazorUI.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddCustomerApiHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetOptions<CustomerApiSettings>();

        services.AddHttpClient("CustomerApi", client =>
        {
            client.BaseAddress = new Uri(options!.BaseUrl!);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        });

        services.AddHttpClient<ICustomerApiClient, CustomerApiClient>(client =>
       {
           client.BaseAddress = new Uri(options!.BaseUrl!);
       })
       .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
       {
           UseCookies = false
       })
       .AddHttpMessageHandler<CookieForwardingHandler>();

        return services;
    }
}