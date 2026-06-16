using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Account;
using System.Text.Json;

namespace CustomerApi.BlazorUI.Services.ApiClients.Account;

public sealed class AccountApiClient(HttpClient httpClient) : IAccountApiClient
{
    private const string BaseRoute = "api/account";

    public async Task<ApiResponse> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PostAsJsonAsync($"{BaseRoute}/changeemail", request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PostAsJsonAsync($"{BaseRoute}/changepassword", request, cancellationToken), cancellationToken);
    }

    private static async Task<ApiResponse> SendAsync(
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

    private static ApiResponse CreateEmptyResponse(HttpResponseMessage response)
    {
        return new ApiResponse
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode ? [] : [new ApiErrorResponse { Message = $"A API retornou {response.StatusCode}." }]
        };
    }

    private static bool TryCreateAuthenticationResponse(string content, out ApiResponse response)
    {
        response = default!;

        try
        {
            var authenticationError = JsonSerializer.Deserialize<AuthenticationErrorResponse>(
                content,
                JsonSerializerOptions.Web);

            if (string.IsNullOrWhiteSpace(authenticationError?.ErrorCode))
                return false;

            response = new ApiResponse
            {
                Success = false,
                StatusCode = authenticationError.Status,
                Errors = [new ApiErrorResponse { Message = GetAuthenticationMessage(authenticationError) }]
            };

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
