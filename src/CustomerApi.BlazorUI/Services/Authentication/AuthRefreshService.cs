namespace CustomerApi.BlazorUI.Services.Authentication;

using Microsoft.Net.Http.Headers;

public sealed class AuthRefreshService(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return false;

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refreshtoken");

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, userAgent);

        AuthCookieRelay.AddRefreshTokenCookie(httpContext, request);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        AuthCookieRelay.StoreRefreshedAuthCookies(httpContext, response);
        AuthCookieRelay.AppendSetCookieHeaders(httpContext, response);

        return true;
    }
}
