using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace CustomerApi.BlazorUI.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomerApiAuthentication(
      this IServiceCollection services,
      IConfiguration configuration)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = configuration["Authentication:CookieName"] ?? "customerapi_ui_auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.LoginPath = configuration["Authentication:LoginPath"] ?? "/login";
                    options.AccessDeniedPath = configuration["Authentication:AccessDeniedPath"] ?? "/access-denied";
                    options.SlidingExpiration = true;
                });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider,
                           ServerAuthenticationStateProvider>();

        return services;
    }
}