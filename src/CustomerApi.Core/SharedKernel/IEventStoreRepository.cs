namespace CustomerApi.Core.SharedKernel;

public interface IEventStoreRepository : IDisposable
{
    Task StoreAsync(IEquatable<EventStore> eventStores);
}
