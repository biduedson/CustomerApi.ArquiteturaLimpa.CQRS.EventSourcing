using System.Text.Json;
using CustomerApi.BlazorUI.Abstractions;
using CustomerApi.BlazorUI.Abstractions.ApiClients;
using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace CustomerApi.BlazorUI.Services.ApiClients;

public abstract class BaseApiClient<TCreateRequest, TUpdateRequest, TKey, T>(
    HttpClient httpClient,
    AuthRefreshService authRefreshService,
    NavigationManager navigation,
    string baseRoute)
       : IApiClient<TCreateRequest, TUpdateRequest, TKey, T>
    where TCreateRequest : IRequest
    where TUpdateRequest : IRequest
    where TKey : IEquatable<TKey>
    where T : class
{
    private const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";
    private const string AccessTokenMissing = "ACCESS_TOKEN_MISSING";
    private const string AccessTokenInvalid = "ACCESS_TOKEN_INVALID";
    private const string AccessForbidden = "ACCESS_FORBIDDEN";

    private readonly AuthRefreshService _authRefreshService = authRefreshService;
    private readonly NavigationManager _navigation = navigation;

    protected HttpClient HttpClient { get; } = httpClient;

    protected string BaseRoute { get; } = baseRoute;

    public async Task<ApiResponse> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PostAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateAsync(TUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PutAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.DeleteAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<T>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync<T>(
            () => HttpClient.GetAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync<IEnumerable<T>>(
            () => HttpClient.GetAsync(BaseRoute, cancellationToken), cancellationToken);
    }

    protected async Task<ApiResponse<TModel>> SendWithAuthAsync<TModel>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync<TModel>(request, cancellationToken);

        // Resposta com sucesso: nao precisa tratar autenticacao.
        if (response.Success)
            return response;

        // Erros comuns da API voltam direto para a pagina.
        if (string.IsNullOrWhiteSpace(response.ErrorCode))
            return response;

        // Tenta refresh/retry; se falhar vai para login, e acesso negado vai para access-denied.
        switch (response.ErrorCode)
        {
            case AccessTokenExpired:
            case AccessTokenMissing:
                if (await _authRefreshService.TryRefreshAsync(cancellationToken))
                    return await SendAsync<TModel>(request, cancellationToken);

                _navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessTokenInvalid:
                _navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessForbidden:
                _navigation.NavigateTo("/access-denied", forceLoad: true);
                return response;

            default:
                return response;
        }
    }

    protected async Task<ApiResponse> SendWithAuthAsync(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync(request, cancellationToken);

        // Resposta com sucesso: nao precisa tratar autenticacao.
        if (response.Success)
            return response;

        // Erros comuns da API voltam direto para a pagina.
        if (string.IsNullOrWhiteSpace(response.ErrorCode))
            return response;

        // Tenta refresh/retry; se falhar vai para login, e acesso negado vai para access-denied.
        switch (response.ErrorCode)
        {
            case AccessTokenExpired:
            case AccessTokenMissing:
                if (await _authRefreshService.TryRefreshAsync(cancellationToken))
                    return await SendAsync(request, cancellationToken);

                _navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessTokenInvalid:
                _navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessForbidden:
                _navigation.NavigateTo("/access-denied", forceLoad: true);
                return response;

            default:
                return response;
        }
    }

    protected static async Task<ApiResponse<TModel>> SendAsync<TModel>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        using var response = await request();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse<TModel>(response);

        return JsonSerializer.Deserialize<ApiResponse<TModel>>(content, JsonSerializerOptions.Web)
            ?? CreateEmptyResponse<TModel>(response);
    }

    protected static async Task<ApiResponse> SendAsync(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        using var response = await request();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse(response);

        return JsonSerializer.Deserialize<ApiResponse>(content, JsonSerializerOptions.Web)
            ?? CreateEmptyResponse(response);
    }

    private static ApiResponse<TModel> CreateEmptyResponse<TModel>(HttpResponseMessage response)
    {
        return new ApiResponse<TModel>
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode
                ? []
                : [new ApiErrorResponse { Message = $"A API retornou {response.StatusCode}." }]
        };
    }

    private static ApiResponse CreateEmptyResponse(HttpResponseMessage response)
    {
        return new ApiResponse
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode
                ? []
                : [new ApiErrorResponse { Message = $"A API retornou {response.StatusCode}." }]
        };
    }
}
