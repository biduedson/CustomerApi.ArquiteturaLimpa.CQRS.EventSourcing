using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Customers;

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
            () => httpClient.GetAsync("{BaseRoute}/{id}", cancellationToken), cancellationToken);
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
            () => httpClient.PutAsJsonAsync("{BaseRoute}/{id}", cancellationToken), cancellationToken);
    }
    private static async Task<ApiResponse<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
    {
        using var response = await request();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);
        return apiResponse ?? new ApiResponse<T>
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Errors = ["A API retornou uma resposta vazia."]
        };
    }

    private static async Task<ApiResponse> SendAsync(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
    {
        using var response = await request();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken: cancellationToken);
        return apiResponse ?? new ApiResponse
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode ? [] : ["A API retornou uma resposta vazia."]
        };
    }
}