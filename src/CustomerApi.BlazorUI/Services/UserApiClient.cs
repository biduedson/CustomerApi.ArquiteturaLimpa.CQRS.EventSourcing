using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Users;
using System.Text.Json;

namespace CustomerApi.BlazorUI.Services;

public sealed class UserApiClient(HttpClient httpClient) : IUserApiClient
{
    private const string BaseRoute = "api/users";

    public async Task<ApiResponse<IEnumerable<UserListItem>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<IEnumerable<UserListItem>>(
            () => httpClient.GetAsync(BaseRoute, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<UserListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SendAsync<UserListItem>(
            () => httpClient.GetAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PostAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PutAsJsonAsync($"{BaseRoute}/profile", request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PutAsJsonAsync($"{BaseRoute}/role", request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.DeleteAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    private static async Task<ApiResponse<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        using var response = await request();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse<T>(response);

        return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonSerializerOptions.Web)
            ?? CreateEmptyResponse<T>(response);
    }

    private static async Task<ApiResponse> SendAsync(
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

    private static ApiResponse<T> CreateEmptyResponse<T>(HttpResponseMessage response)
    {
        return new ApiResponse<T>
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode ? [] : [new ApiErrorResponse { Message = $"A API retornou {response.StatusCode}." }]
        };
    }

    private static ApiResponse CreateEmptyResponse(HttpResponseMessage response)
    {
        return new ApiResponse
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode ? [] : [new ApiErrorResponse { Message = $"A API retornou {response.StatusCode}." }]
        };
    }
}
