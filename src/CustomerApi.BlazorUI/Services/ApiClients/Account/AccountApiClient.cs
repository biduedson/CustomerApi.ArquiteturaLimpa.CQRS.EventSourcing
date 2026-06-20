using System.Text.Json;
using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Account;
using CustomerApi.BlazorUI.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace CustomerApi.BlazorUI.Services.ApiClients.Account;

public sealed class AccountApiClient(
    HttpClient httpClient,
    AuthRefreshService authRefreshService,
    NavigationManager navigation) : IAccountApiClient
{
    private const string BaseRoute = "api/account";
    private const string AccessTokenExpired = "ACCESS_TOKEN_EXPIRED";
    private const string AccessTokenMissing = "ACCESS_TOKEN_MISSING";
    private const string AccessTokenInvalid = "ACCESS_TOKEN_INVALID";
    private const string AccessForbidden = "ACCESS_FORBIDDEN";

    public async Task<ApiResponse> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => httpClient.PostAsJsonAsync($"{BaseRoute}/changeemail", request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => httpClient.PostAsJsonAsync($"{BaseRoute}/changepassword", request, cancellationToken), cancellationToken);
    }

    private async Task<ApiResponse> SendWithAuthAsync(
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
                if (await authRefreshService.TryRefreshAsync(cancellationToken))
                    return await SendAsync(request, cancellationToken);

                navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessTokenInvalid:
                navigation.NavigateTo("/login", forceLoad: true);
                return response;

            case AccessForbidden:
                navigation.NavigateTo("/access-denied", forceLoad: true);
                return response;

            default:
                return response;
        }
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
