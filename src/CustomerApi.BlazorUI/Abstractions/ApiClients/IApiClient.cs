using CustomerApi.BlazorUI.Abstractions;
using CustomerApi.BlazorUI.Models;

namespace CustomerApi.BlazorUI.Abstractions.ApiClients;

public interface IApiClient<TCreateRequest, TUpdateRequest, in TKey, T>
    where TCreateRequest : IRequest
    where TUpdateRequest : IRequest
    where TKey : IEquatable<TKey>
    where T : class
{
    Task<ApiResponse> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateAsync(TUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<ApiResponse<T>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
}
