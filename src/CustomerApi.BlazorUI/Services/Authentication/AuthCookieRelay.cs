using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Services.Authentication;

internal static class AuthCookieRelay
{
    private const string AccessTokenCookie = "access_Token";
    private const string RefreshTokenCookie = "refresh_Token";
    private const string RefreshedAccessTokenItem = "RefreshedAccessToken";
    private const string RefreshedRefreshTokenItem = "RefreshedRefreshToken";

    public static void AppendSetCookieHeaders(
        HttpContext httpContext,
        HttpResponseMessage apiResponse)
    {
        if (httpContext.Response.HasStarted)
            return;

        if (!apiResponse.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies))
            return;

        foreach (var cookie in cookies)
            httpContext.Response.Headers.Append(HeaderNames.SetCookie, cookie);
    }

    public static void AddRefreshTokenCookie(
        HttpContext httpContext,
        HttpRequestMessage request)
    {
        var refreshToken = GetCookieValue(httpContext, RefreshTokenCookie, RefreshedRefreshTokenItem);

        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        request.Headers.TryAddWithoutValidation(
            HeaderNames.Cookie,
            $"{RefreshTokenCookie}={refreshToken}");
    }

    public static void AddAuthCookies(
        HttpContext httpContext,
        HttpRequestMessage request)
    {
        var cookies = new List<string>();

        var accessToken = GetCookieValue(httpContext, AccessTokenCookie, RefreshedAccessTokenItem);
        if (!string.IsNullOrWhiteSpace(accessToken))
            cookies.Add($"{AccessTokenCookie}={accessToken}");

        var refreshToken = GetCookieValue(httpContext, RefreshTokenCookie, RefreshedRefreshTokenItem);
        if (!string.IsNullOrWhiteSpace(refreshToken))
            cookies.Add($"{RefreshTokenCookie}={refreshToken}");

        if (cookies.Count == 0)
            return;

        request.Headers.TryAddWithoutValidation(
            HeaderNames.Cookie,
            string.Join("; ", cookies));
    }

    public static void StoreRefreshedAuthCookies(
        HttpContext httpContext,
        HttpResponseMessage apiResponse)
    {
        var accessToken = GetCookieValue(apiResponse.Headers, AccessTokenCookie);
        if (!string.IsNullOrWhiteSpace(accessToken))
            httpContext.Items[RefreshedAccessTokenItem] = accessToken;

        var refreshToken = GetCookieValue(apiResponse.Headers, RefreshTokenCookie);
        if (!string.IsNullOrWhiteSpace(refreshToken))
            httpContext.Items[RefreshedRefreshTokenItem] = refreshToken;
    }

    public static void DeleteAuthCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(AccessTokenCookie, new CookieOptions { Path = "/" });
        httpContext.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/" });
        httpContext.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/auth" });
    }

    public static string? GetCookieValue(HttpResponseHeaders headers, string cookieName)
    {
        if (!headers.TryGetValues(HeaderNames.SetCookie, out var cookieHeaders))
            return null;

        var setCookieHeaders = SetCookieHeaderValue.ParseList(cookieHeaders.ToList());
        return setCookieHeaders
            .FirstOrDefault(cookie => string.Equals(cookie.Name.Value, cookieName, StringComparison.OrdinalIgnoreCase))
            ?.Value.Value;
    }

    private static string? GetCookieValue(
        HttpContext httpContext,
        string cookieName,
        string refreshedItemName)
    {
        if (httpContext.Items.TryGetValue(refreshedItemName, out var refreshedValue))
            return refreshedValue?.ToString();

        return httpContext.Request.Cookies[cookieName];
    }
}
