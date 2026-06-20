using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class CustomerApiRequestHandler(
    IHttpContextAccessor httpContextAccessor,
    AuthCookieService authCookieService) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            authCookieService.AddAuthCookies(httpContext, request);
            AddUserAgent(httpContext, request);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static void AddUserAgent(HttpContext httpContext, HttpRequestMessage request)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, userAgent);
    }
}
