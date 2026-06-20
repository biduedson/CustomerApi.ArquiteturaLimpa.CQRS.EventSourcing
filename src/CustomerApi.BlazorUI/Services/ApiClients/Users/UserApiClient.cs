using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Users;
using CustomerApi.BlazorUI.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace CustomerApi.BlazorUI.Services.ApiClients.Users;

public sealed class UserApiClient(
    HttpClient httpClient,
    AuthRefreshService authRefreshService,
    NavigationManager navigation)
    : BaseApiClient<CreateUserRequest, UpdateUserProfileRequest, Guid, UserListItem>(
        httpClient,
        authRefreshService,
        navigation,
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
