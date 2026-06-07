using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CustomerApi.BlazorUI.Extensions.EndpointRouteBuilderExtensions;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var form = await httpContext.Request.ReadFormAsync();
            var email = form["email"].ToString();
            var password = form["password"].ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Results.Redirect("/login?error=required");


            var client = httpClientFactory.CreateClient("CustomerApi");

            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            using var apiResponse = await client.PostAsJsonAsync("api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!apiResponse.IsSuccessStatusCode)
                return Results.Redirect("/login?error=invalid");

            if (apiResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
                foreach (var cookie in cookies)
                    httpContext.Response.Headers.Append("Set-Cookie", cookie);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, email),
                new(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Redirect("/customers");
        });

        app.MapPost("/auth/logout", async (
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient("CustomerApi");
            try
            {
                await client.PostAsync("api/auth/logout", content: null);
            }
            catch
            {
            }

            httpContext.Response.Cookies.Delete("access_token");
            httpContext.Response.Cookies.Delete("refresh_token");

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        return app;
    }
}
