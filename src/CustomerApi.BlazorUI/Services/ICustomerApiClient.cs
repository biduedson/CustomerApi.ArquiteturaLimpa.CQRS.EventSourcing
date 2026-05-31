using CustomerApi.BlazorUI.Models;
using CustomerApi.BlazorUI.Models.Customers;

namespace CustomerApi.BlazorUI.Services;

public interface ICustomerApiClient
{
    Task<ApiResponse<IEnumerable<CustomerListItem>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}