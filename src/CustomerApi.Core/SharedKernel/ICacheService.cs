namespace CustomerApi.Core.SharedKernel;

public interface ICacheService
{
    Task<TItem> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> factory);
    Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<IReadOnlyList<TItem>>> factory);
    Task RemoveAsync(params string[] cacheKeys);
}
