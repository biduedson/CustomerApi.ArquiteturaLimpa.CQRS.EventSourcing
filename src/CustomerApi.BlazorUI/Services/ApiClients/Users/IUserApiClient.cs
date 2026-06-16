using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Users;

namespace CustomerApi.BlazorUI.Services.ApiClients.Users;

public interface IUserApiClient
{
    Task<ApiResponse<IEnumerable<UserListItem>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<UserListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
