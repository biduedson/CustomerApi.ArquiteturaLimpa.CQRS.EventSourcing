namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class CookieForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
            AuthCookieRelay.AddAuthCookies(httpContext, request);

        return base.SendAsync(request, cancellationToken);
    }
}
