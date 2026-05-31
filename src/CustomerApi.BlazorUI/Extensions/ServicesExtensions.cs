using CustomerApi.BlazorUI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace CustomerApi.BlazorUI.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddCustomerApiServices(
       this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddScoped<ToastService>();
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

        return services;
    }
}