using System.Text.Json;
using CustomerApi.BlazorUI.Abstractions;
using CustomerApi.BlazorUI.Abstractions.ApiClients;
using CustomerApi.BlazorUI.Models;

namespace CustomerApi.BlazorUI.Services.ApiClients;

public abstract class BaseApiClient<TCreateRequest, TUpdateRequest, TKey, T>(HttpClient httpClient, string baseRoute)
       : IApiClient<TCreateRequest, TUpdateRequest, TKey, T>
    where TCreateRequest : IRequest
    where TUpdateRequest : IRequest
    where TKey : IEquatable<TKey>
    where T : class
{
    protected HttpClient HttpClient { get; } = httpClient;

    protected string BaseRoute { get; } = baseRoute;

    public async Task<ApiResponse> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => HttpClient.PostAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateAsync(TUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => HttpClient.PutAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => HttpClient.DeleteAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<T>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(
            () => HttpClient.GetAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<IEnumerable<T>>(
            () => HttpClient.GetAsync(BaseRoute, cancellationToken), cancellationToken);
    }

    protected static async Task<ApiResponse<TModel>> SendAsync<TModel>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        using var response = await request();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse<TModel>(response);

        if (TryCreateAuthenticationResponse<TModel>(content, out var authenticationResponse))
            return authenticationResponse;

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

        if (TryCreateAuthenticationResponse(content, out var authenticationResponse))
            return authenticationResponse;

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

    private static bool TryCreateAuthenticationResponse<TModel>(
        string content,
        out ApiResponse<TModel> response)
    {
        response = default!;

        if (!TryReadAuthenticationError(content, out var authenticationError))
            return false;

        response = new ApiResponse<TModel>
        {
            Success = false,
            StatusCode = authenticationError.Status,
            Errors = [new ApiErrorResponse { Message = GetAuthenticationMessage(authenticationError) }]
        };

        return true;
    }

    private static bool TryCreateAuthenticationResponse(string content, out ApiResponse response)
    {
        response = default!;

        if (!TryReadAuthenticationError(content, out var authenticationError))
            return false;

        response = new ApiResponse
        {
            Success = false,
            StatusCode = authenticationError.Status,
            Errors = [new ApiErrorResponse { Message = GetAuthenticationMessage(authenticationError) }]
        };

        return true;
    }

    private static bool TryReadAuthenticationError(
        string content,
        out AuthenticationErrorResponse authenticationError)
    {
        authenticationError = default!;

        try
        {
            var error = JsonSerializer.Deserialize<AuthenticationErrorResponse>(
                content,
                JsonSerializerOptions.Web);

            if (string.IsNullOrWhiteSpace(error?.ErrorCode))
                return false;

            authenticationError = error;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string GetAuthenticationMessage(AuthenticationErrorResponse authenticationError)
    {
        if (!string.IsNullOrWhiteSpace(authenticationError.Detail))
            return authenticationError.Detail;

        return string.IsNullOrWhiteSpace(authenticationError.Title)
            ? "Não foi possível validar sua sessão."
            : authenticationError.Title;
    }
}
