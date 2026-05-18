using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Data.Context;

namespace CustomerApi.Infrastructure.Data.Repositories;

internal sealed class EventStoreRepository(EventStoreDbContext dbContext) : IEventStoreRepository
{
    public async Task StoreAsync(IEnumerable<EventStore> eventStores)
    {
        await dbContext.EventStores.AddRangeAsync(eventStores);
        await dbContext.SaveChangesAsync();
    }

    #region IDisposable
    private bool _disposed;

    ~EventStoreRepository() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if(disposing) dbContext.Dispose();

        _disposed = true;
    }
    #endregion
}
