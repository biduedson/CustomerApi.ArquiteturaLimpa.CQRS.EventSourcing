namespace CustomerApi.Core.SharedKernel;

public interface IEventStoreRepository : IDisposable
{
    Task StoreAsync(IEnumerable<EventStore> eventStores);
}
