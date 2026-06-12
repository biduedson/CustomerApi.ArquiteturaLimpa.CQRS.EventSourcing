using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Extensions.EndpointRouteBuilderExtensions;

public static class AuthEndpointExtensions
{
    private const string AccessTokenCookie = "access_Token";
    private const string RefreshTokenCookie = "refresh_Token";

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

            var accessToken = GetCookieValue(apiResponse.Headers, AccessTokenCookie);
            var claims = CreateClaims(accessToken, email);

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

            httpContext.Response.Cookies.Delete(AccessTokenCookie);
            httpContext.Response.Cookies.Delete(RefreshTokenCookie);

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        return app;
    }

    private static List<Claim> CreateClaims(string? accessToken, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email)
        };

        if (string.IsNullOrWhiteSpace(accessToken))
            return claims;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        AddClaimIfExists(claims, jwt, ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);
        AddClaimIfExists(claims, jwt, ClaimTypes.Name, ClaimTypes.Name, JwtRegisteredClaimNames.Name);
        AddClaimIfExists(claims, jwt, ClaimTypes.Email, ClaimTypes.Email, JwtRegisteredClaimNames.Email);

        var roles = jwt.Claims
            .Where(claim =>
                claim.Type == ClaimTypes.Role ||
                claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    private static void AddClaimIfExists(
        List<Claim> claims,
        JwtSecurityToken jwt,
        string claimType,
        params string[] tokenClaimTypes)
    {
        var value = jwt.Claims
            .FirstOrDefault(claim => tokenClaimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            ?.Value;

        if (!string.IsNullOrWhiteSpace(value))
            claims.Add(new Claim(claimType, value));
    }

    private static string? GetCookieValue(HttpResponseHeaders headers, string cookieName)
    {
        if (!headers.TryGetValues(HeaderNames.SetCookie, out var cookieHeaders))
            return null;

        var setCookieHeaders = SetCookieHeaderValue.ParseList(cookieHeaders.ToList());
        return setCookieHeaders
            .FirstOrDefault(cookie => string.Equals(cookie.Name.Value, cookieName, StringComparison.OrdinalIgnoreCase))
            ?.Value.Value;
    }
}
