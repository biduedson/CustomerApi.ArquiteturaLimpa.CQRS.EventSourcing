using System.Net;
using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class AuthenticationRefreshHandler(
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    private const string RefreshTokenRoute = "api/auth/refreshtoken";
    private const string ExpiredTokenError = "Token expirado";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var retryRequest = await CloneAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized
            || IsRefreshTokenRequest(request)
            || !await IsExpiredTokenResponseAsync(response, cancellationToken))
        {
            retryRequest.Dispose();
            return response;
        }

        var refreshedCookieHeader = await TryRefreshTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(refreshedCookieHeader))
        {
            retryRequest.Dispose();
            return response;
        }

        response.Dispose();
        retryRequest.Headers.Remove(HeaderNames.Cookie);
        retryRequest.Headers.TryAddWithoutValidation(HeaderNames.Cookie, refreshedCookieHeader);

        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static async Task<bool> IsExpiredTokenResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
            return false;

        try
        {
            using var document = JsonDocument.Parse(content);

            return document.RootElement.TryGetProperty("error", out var error)
                && string.Equals(error.GetString(), ExpiredTokenError, StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var currentCookieHeader = httpContext?.Request.Headers.Cookie.ToString();

        if (httpContext is null || string.IsNullOrWhiteSpace(currentCookieHeader))
            return null;

        var client = httpClientFactory.CreateClient("CustomerApi");
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, RefreshTokenRoute);
        refreshRequest.Headers.TryAddWithoutValidation(HeaderNames.Cookie, currentCookieHeader);

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
            refreshRequest.Headers.UserAgent.ParseAdd(userAgent);

        using var refreshResponse = await client.SendAsync(refreshRequest, cancellationToken);

        if (!refreshResponse.IsSuccessStatusCode
            || !refreshResponse.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookieHeaders))
        {
            return null;
        }

        var normalizedSetCookieHeaders = setCookieHeaders
            .Select(AuthCookieHeaderHelper.NormalizeForUi)
            .ToList();

        if (!httpContext.Response.HasStarted)
            foreach (var setCookieHeader in normalizedSetCookieHeaders)
                httpContext.Response.Headers.Append(HeaderNames.SetCookie, setCookieHeader);

        return AuthCookieHeaderHelper.CreateCookieHeader(currentCookieHeader, normalizedSetCookieHeaders);
    }

    private static bool IsRefreshTokenRequest(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath.TrimStart('/');
        return string.Equals(path, RefreshTokenRoute, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpRequestMessage> CloneAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        return clone;
    }
}
