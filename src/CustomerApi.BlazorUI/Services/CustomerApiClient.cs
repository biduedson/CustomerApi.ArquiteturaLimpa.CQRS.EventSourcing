using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Customers;
using System.Text.Json;

namespace CustomerApi.BlazorUI.Services;

public sealed class CustomerApiClient(HttpClient httpClient) : ICustomerApiClient
{
    private const string BaseRoute = "api/customers";

    public async Task<ApiResponse<IEnumerable<CustomerListItem>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<IEnumerable<CustomerListItem>>(
            () => httpClient.GetAsync(BaseRoute, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse<CustomerListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SendAsync<CustomerListItem>(
            () => httpClient.GetAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PostAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.PutAsJsonAsync(BaseRoute, request, cancellationToken), cancellationToken);
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            () => httpClient.DeleteAsync($"{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }
    private static async Task<ApiResponse<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
    {
        using var response = await request();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return CreateEmptyResponse<T>(response);

        return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonSerializerOptions.Web)
            ?? CreateEmptyResponse<T>(response);
    }

    private static async Task<ApiResponse> SendAsync(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
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
