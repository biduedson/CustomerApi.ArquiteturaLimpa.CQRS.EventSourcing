using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Users;
using CustomerApi.BlazorUI.Services.Authentication;

namespace CustomerApi.BlazorUI.Services.ApiClients.Users;

public sealed class UserApiClient(
    HttpClient httpClient,
    ApiResponseAuthHandler authHandler)
    : BaseApiClient<CreateUserRequest, UpdateUserProfileRequest, Guid, UserListItem>(
        httpClient,
        authHandler,
        Route),
        IUserApiClient
{
    private const string Route = "api/users";

    public async Task<ApiResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PutAsJsonAsync($"{Route}/profile", request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PutAsJsonAsync($"{Route}/role", request, cancellationToken), cancellationToken);
    }
}
