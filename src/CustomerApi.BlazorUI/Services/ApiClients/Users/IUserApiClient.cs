using CustomerApi.BlazorUI.Abstractions.ApiClients;
using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Users;

namespace CustomerApi.BlazorUI.Services.ApiClients.Users;

public interface IUserApiClient
    : IApiClient<CreateUserRequest, UpdateUserProfileRequest, Guid, UserListItem>
{
    Task<ApiResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
}
