using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Services.Authentication;

internal static class AuthCookieHeaderHelper
{
    private const string RefreshTokenCookie = "refresh_Token";

    public static string NormalizeForUi(string setCookieHeader)
    {
        var cookie = SetCookieHeaderValue.Parse(setCookieHeader);

        if (string.Equals(cookie.Name.Value, RefreshTokenCookie, StringComparison.OrdinalIgnoreCase))
            cookie.Path = "/";

        return cookie.ToString();
    }

    public static string CreateCookieHeader(string? currentCookieHeader, IEnumerable<string> setCookieHeaders)
    {
        var cookies = ParseCookieHeader(currentCookieHeader);
        var setCookies = SetCookieHeaderValue.ParseList(setCookieHeaders.ToList());

        foreach (var setCookie in setCookies)
        {
            if (!string.IsNullOrWhiteSpace(setCookie.Name.Value))
                cookies[setCookie.Name.Value] = setCookie.Value.Value ?? string.Empty;
        }

        return string.Join("; ", cookies.Select(cookie => $"{cookie.Key}={cookie.Value}"));
    }

    private static Dictionary<string, string> ParseCookieHeader(string? cookieHeader)
    {
        var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(cookieHeader))
            return cookies;

        foreach (var cookiePair in cookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = cookiePair.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var name = cookiePair[..separatorIndex].Trim();
            var value = cookiePair[(separatorIndex + 1)..].Trim();

            if (!string.IsNullOrWhiteSpace(name))
                cookies[name] = value;
        }

        return cookies;
    }
}
