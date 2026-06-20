namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class AuthRefreshService(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    AuthCookieService authCookieService)
{
    public async Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return false;

        using var response = await httpClient.PostAsync("api/auth/refreshtoken", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        authCookieService.RefreshCurrentRequestCookies(httpContext, response);

        return true;
    }
}
