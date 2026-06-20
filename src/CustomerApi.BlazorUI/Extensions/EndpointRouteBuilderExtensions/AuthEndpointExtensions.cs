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
            IHttpClientFactory httpClientFactory,
            AuthCookieService authCookieService) =>
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

            authCookieService.AppendSetCookieHeaders(httpContext, apiResponse);

            var accessToken = authCookieService.GetCookieValue(apiResponse.Headers, "access_Token");
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
            IHttpClientFactory httpClientFactory,
            AuthCookieService authCookieService) =>
        {
            var client = httpClientFactory.CreateClient("CustomerApi");
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");

                authCookieService.AddRefreshTokenCookie(httpContext, request);

                using var apiResponse = await client.SendAsync(request);
            }
            catch
            {
            }

            authCookieService.DeleteAuthCookies(httpContext);

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        app.MapPost("/account/changepassword", async (
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            AuthCookieService authCookieService) =>
        {
            var form = await httpContext.Request.ReadFormAsync();
            var currentPassword = form["currentPassword"].ToString();
            var newPassword = form["newPassword"].ToString();
            var confirmPassword = form["confirmPassword"].ToString();

            if (string.IsNullOrWhiteSpace(currentPassword)
                || string.IsNullOrWhiteSpace(newPassword)
                || string.IsNullOrWhiteSpace(confirmPassword))
                return Results.Redirect("/account?changePasswordError=required");

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
                return Results.Redirect("/account?changePasswordError=confirm");

            var client = httpClientFactory.CreateClient("CustomerApi");

            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/changepassword")
            {
                Content = JsonContent.Create(new
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                })
            };

            authCookieService.AddAuthCookies(httpContext, request);

            using var apiResponse = await client.SendAsync(request);

            if (!apiResponse.IsSuccessStatusCode)
                return Results.Redirect("/account?changePasswordError=invalid");

            authCookieService.AppendSetCookieHeaders(httpContext, apiResponse);
            authCookieService.DeleteAuthCookies(httpContext);

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        return app;
    }

}
