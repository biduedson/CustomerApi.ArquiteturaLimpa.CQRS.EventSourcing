using System.Text.Json;
using CustomerApi.BlazorUI.Abstractions;
using CustomerApi.BlazorUI.Abstractions.ApiClients;
using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Services.Authentication;

namespace CustomerApi.BlazorUI.Services.ApiClients;

public abstract class BaseApiClient<TCreateRequest, TUpdateRequest, TKey, T>(
    HttpClient httpClient,
    ApiResponseAuthHandler authHandler,
    string baseRoute)
       : IApiClient<TCreateRequest, TUpdateRequest, TKey, T>
    where TCreateRequest : IRequest
    where TUpdateRequest : IRequest
    where TKey : IEquatable<TKey>
    where T : class
{
    private readonly ApiResponseAuthHandler _authHandler = authHandler;

    protected HttpClient HttpClient { get; } = httpClient;

    protected string BaseRoute { get; } = baseRoute;

    public async Task<ApiResponse> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PostAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateAsync(TUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.PutAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync(
            () => HttpClient.DeleteAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<T>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync<T>(
            () => HttpClient.GetAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SendWithAuthAsync<IEnumerable<T>>(
            () => HttpClient.GetAsync(BaseRoute, cancellationToken), cancellationToken);
    }

    protected async Task<ApiResponse<TModel>> SendWithAuthAsync<TModel>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        // Primeira tentativa: chama a API e transforma o retorno em ApiResponse<TModel>.
        var response = await SendAsync<TModel>(request, cancellationToken);

        // Se deu certo, encerra aqui. Nao precisa validar refresh, login ou access denied.
        if (response.Success)
            return response;

        // Chama ShouldHandleAuthenticationError para saber se esse ErrorCode pertence
        // ao fluxo de autenticacao: token expirado, ausente, invalido ou acesso negado.
        // Se nao pertence, e um erro comum da API e deve voltar direto para a tela.
        if (!ApiResponseAuthHandler.ShouldHandleAuthenticationError(response.ErrorCode))
            return response;

        // Agora sim entrega para o handler: ele pode renovar o token, redirecionar para
        // login/access-denied ou devolver a resposta original.
        return await _authHandler.HandleAuthenticationErrorAsync(
            response,
            () => SendAsync<TModel>(request, cancellationToken),
            cancellationToken);
    }

    protected async Task<ApiResponse> SendWithAuthAsync(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        // Primeira tentativa: chama a API e transforma o retorno em ApiResponse.
        var response = await SendAsync(request, cancellationToken);

        // Se deu certo, encerra aqui. Nao precisa validar refresh, login ou access denied.
        if (response.Success)
            return response;

        // Chama ShouldHandleAuthenticationError para saber se esse ErrorCode pertence
        // ao fluxo de autenticacao: token expirado, ausente, invalido ou acesso negado.
        // Se nao pertence, e um erro comum da API e deve voltar direto para a tela.
        if (!ApiResponseAuthHandler.ShouldHandleAuthenticationError(response.ErrorCode))
            return response;

        // Agora sim entrega para o handler: ele pode renovar o token, redirecionar para
        // login/access-denied ou devolver a resposta original.
        return await _authHandler.HandleAuthenticationErrorAsync(
            response,
            () => SendAsync(request, cancellationToken),
            cancellationToken);
    }

    protected static async Task<ApiResponse<TModel>> SendAsync<TModel>(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        // Executa a requisicao HTTP; o Func cria uma nova chamada quando precisar repetir.
        using var response = await request();

        // Converte o JSON da API para o modelo ApiResponse<TModel> usado pela UI.
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse<TModel>(response);

        return JsonSerializer.Deserialize<ApiResponse<TModel>>(content, JsonSerializerOptions.Web)
            ?? CreateEmptyResponse<TModel>(response);
    }

    protected static async Task<ApiResponse> SendAsync(
        Func<Task<HttpResponseMessage>> request,
        CancellationToken cancellationToken)
    {
        // Executa a requisicao HTTP; o Func cria uma nova chamada quando precisar repetir.
        using var response = await request();

        // Converte o JSON da API para o modelo ApiResponse usado pela UI.
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse(response);

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
}
