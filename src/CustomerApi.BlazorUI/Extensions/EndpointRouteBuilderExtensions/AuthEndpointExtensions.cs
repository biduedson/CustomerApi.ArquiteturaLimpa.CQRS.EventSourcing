using System.Security.Claims;
using CustomerApi.BlazorUI.Services.Authentication;
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

            AuthCookieRelay.AppendSetCookieHeaders(httpContext, apiResponse);

            var accessToken = AuthCookieRelay.GetCookieValue(apiResponse.Headers, "access_Token");
            var claims = AuthClaimsFactory.CreateClaims(accessToken, email);

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Redirect("/customers");
        });

        app.MapPost("/api/auth/logout", async (
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient("CustomerApi");
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");

                AuthCookieRelay.AddRefreshTokenCookie(httpContext, request);

                using var apiResponse = await client.SendAsync(request);
            }
            catch
            {
            }

            AuthCookieRelay.DeleteAuthCookies(httpContext);

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        app.MapPost("/api/auth/refreshtoken", async (
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient("CustomerApi");

            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refreshtoken");

            AuthCookieRelay.AddRefreshTokenCookie(httpContext, request);

            using var apiResponse = await client.SendAsync(request);

            if (!apiResponse.IsSuccessStatusCode)
                return Results.Unauthorized();

            AuthCookieRelay.AppendSetCookieHeaders(httpContext, apiResponse);

            return Results.Ok();
        });

        return app;
    }

}
