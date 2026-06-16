using System.Net;
using System.Text.Json;
using CustomerApi.BlazorUI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Net.Http.Headers;

namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class AuthenticationRefreshHandler(
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory httpClientFactory,
    NavigationManager navigation) : DelegatingHandler
{
    private const string RefreshTokenRoute = "api/auth/refreshtoken";
    private const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";
    private const string AccessTokenMissing = "ACCESS_TOKEN_MISSING";
    private const string AccessTokenInvalid = "ACCESS_TOKEN_INVALID";
    private const string AccessForbidden = "ACCESS_FORBIDDEN";

    // Intercepta respostas da API e decide se deve renovar, redirecionar ou retornar.
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var retryRequest = await CloneAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return ReturnOriginalResponse(response, retryRequest);

        var authenticationError = await TryReadAuthenticationErrorAsync(response, cancellationToken);

        if (authenticationError is null || IsRefreshTokenRequest(request))
        {
            retryRequest.Dispose();
            return response;
        }

        switch (authenticationError.ErrorCode)
        {
            case AccessTokenExpired:
                return await HandleExpiredAccessTokenAsync(response, retryRequest, cancellationToken);

            case AccessTokenMissing:
            case AccessTokenInvalid:
                NavigateToLogin();
                return ReturnOriginalResponse(response, retryRequest);

            case AccessForbidden:
                NavigateTo("/access-denied");
                return ReturnOriginalResponse(response, retryRequest);

            default:
                return ReturnOriginalResponse(response, retryRequest);
        }
    }

    // Tenta renovar o access token expirado e repete a request original.
    private async Task<HttpResponseMessage> HandleExpiredAccessTokenAsync(
        HttpResponseMessage response,
        HttpRequestMessage retryRequest,
        CancellationToken cancellationToken)
    {
        var refreshedCookieHeader = await TryRefreshTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(refreshedCookieHeader))
        {
            NavigateToLogin();
            retryRequest.Dispose();
            return response;
        }

        response.Dispose();
        retryRequest.Headers.Remove(HeaderNames.Cookie);
        retryRequest.Headers.TryAddWithoutValidation(HeaderNames.Cookie, refreshedCookieHeader);

        return await base.SendAsync(retryRequest, cancellationToken);
    }

    // Descarta a request de retry nao usada e retorna a resposta original.
    private static HttpResponseMessage ReturnOriginalResponse(
        HttpResponseMessage response,
        HttpRequestMessage retryRequest)
    {
        retryRequest.Dispose();
        return response;
    }

    // Le o payload de erro JWT retornado pela WebApi, quando existir.
    private static async Task<AuthenticationErrorResponse?> TryReadAuthenticationErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode is not HttpStatusCode.Unauthorized
            && response.StatusCode is not HttpStatusCode.Forbidden)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            var authenticationError = JsonSerializer.Deserialize<AuthenticationErrorResponse>(
                content,
                JsonSerializerOptions.Web);

            return string.IsNullOrWhiteSpace(authenticationError?.ErrorCode)
                ? null
                : authenticationError;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Limpa os cookies locais antes de enviar o usuario para o login.
    private void NavigateToLogin()
    {
        ClearAuthenticationCookies();
        NavigateTo("/login");
    }

    // Redireciona com seguranca no prerender ou no Blazor ja interativo.
    private void NavigateTo(string path)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Response.HasStarted is false)
        {
            httpContext.Response.Redirect(path);
            return;
        }

        try
        {
            navigation.NavigateTo(path, forceLoad: true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    // Remove os cookies de access e refresh token da resposta da UI.
    private void ClearAuthenticationCookies()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Response.HasStarted is false)
        {
            httpContext.Response.Cookies.Delete("access_Token", new CookieOptions { Path = "/" });
            httpContext.Response.Cookies.Delete("refresh_Token", new CookieOptions { Path = "/" });
        }
    }

    // Chama o endpoint de refresh da WebApi e retorna o Cookie atualizado.
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

    // Evita que a propria request de refresh tente renovar token de novo.
    private static bool IsRefreshTokenRequest(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath.TrimStart('/');
        return string.Equals(path, RefreshTokenRoute, StringComparison.OrdinalIgnoreCase);
    }

    // Copia a request para poder repeti-la depois do refresh com sucesso.
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
