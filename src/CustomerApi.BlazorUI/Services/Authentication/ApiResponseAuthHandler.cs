using CustomerApi.BlazorUI.Models;
using Microsoft.AspNetCore.Components;

namespace CustomerApi.BlazorUI.Services.Authentication;

public sealed class ApiResponseAuthHandler(
    AuthRefreshService authRefresh,
    NavigationManager navigation)
{
    private const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";
    private const string AccessTokenMissing = "ACCESS_TOKEN_MISSING";
    private const string AccessTokenInvalid = "ACCESS_TOKEN_INVALID";
    private const string AccessForbidden = "ACCESS_FORBIDDEN";

    public static bool ShouldHandleAuthenticationError(string? errorCode) =>
        errorCode is
            AccessTokenExpired or
            AccessTokenMissing or
            AccessTokenInvalid or
            AccessForbidden;

    public async Task<ApiResponse<T>> HandleAuthenticationErrorAsync<T>(
        ApiResponse<T> response,
        Func<Task<ApiResponse<T>>> retry,
        CancellationToken cancellationToken = default)
    {
        if (response.ErrorCode is AccessTokenExpired or AccessTokenMissing)
        {
            if (await authRefresh.TryRefreshAsync(cancellationToken))
                return await retry();

            navigation.NavigateTo("/login", forceLoad: true);
            return response;
        }

        if (response.ErrorCode == AccessTokenInvalid)
            navigation.NavigateTo("/login", forceLoad: true);

        if (response.ErrorCode == AccessForbidden)
            navigation.NavigateTo("/access-denied", forceLoad: true);

        return response;
    }

    public async Task<ApiResponse> HandleAuthenticationErrorAsync(
        ApiResponse response,
        Func<Task<ApiResponse>> retry,
        CancellationToken cancellationToken = default)
    {
        if (response.ErrorCode is AccessTokenExpired or AccessTokenMissing)
        {
            if (await authRefresh.TryRefreshAsync(cancellationToken))
                return await retry();

            navigation.NavigateTo("/login", forceLoad: true);
            return response;
        }

        if (response.ErrorCode == AccessTokenInvalid)
            navigation.NavigateTo("/login", forceLoad: true);

        if (response.ErrorCode == AccessForbidden)
            navigation.NavigateTo("/access-denied", forceLoad: true);

        return response;
    }
}
