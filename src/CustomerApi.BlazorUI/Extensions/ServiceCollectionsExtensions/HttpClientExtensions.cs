using CustomerApi.BlazorUI.Extensions.ConfigurationExtensions;
using CustomerApi.BlazorUI.Services.ApiClients.Account;
using CustomerApi.BlazorUI.Services.ApiClients.Customers;
using CustomerApi.BlazorUI.Services.ApiClients.Users;
using CustomerApi.BlazorUI.Services.Authentication;
using CustomerApi.BlazorUI.Settings;

namespace CustomerApi.BlazorUI.Extensions.ServiceCollectionsExtensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddCustomerApiHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetOptions<CustomerApiSettings>();

        services.AddScoped<ApiResponseAuthHandler>();
        services.AddTransient<CookieForwardingHandler>();

        services.AddHttpClient<AuthRefreshService>(client =>
        {
            client.BaseAddress = new Uri(options!.BaseUrl!);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        });

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

        services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
        {
            client.BaseAddress = new Uri(options!.BaseUrl!);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false
        })
        .AddHttpMessageHandler<CookieForwardingHandler>();

        services.AddHttpClient<IAccountApiClient, AccountApiClient>(client =>
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
