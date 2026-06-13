using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Account;

namespace CustomerApi.BlazorUI.Services;

public interface IAccountApiClient
{
    Task<ApiResponse> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
