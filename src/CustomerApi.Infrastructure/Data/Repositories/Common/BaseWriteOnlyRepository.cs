using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Repositories.Common;

internal abstract class BaseWriteOnlyRepository<TEntity, TKey>(WriteDbContext dbContext) : IWriteOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    private static readonly Func<WriteDbContext, TKey, Task<TEntity>> GetByIdCompiledAsync =
        EF.CompileAsyncQuery((WriteDbContext dbContext, TKey id) =>
        dbContext
        .Set<TEntity>()
        .AsNoTrackingWithIdentityResolution()
        .FirstOrDefault(entity => entity.Id.Equals(id))!);

    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();
    protected readonly WriteDbContext DbContext = dbContext;

    public void Add(TEntity entity) =>
       _dbSet.Add(entity);

    public void Update(TEntity entity) =>
       _dbSet.Update(entity);

    public void Remove(TEntity entity) =>
       _dbSet.Remove(entity);
    public async Task<TEntity> GetByIdAsync(TKey id) =>
        await GetByIdCompiledAsync(DbContext, id);

    #region IDisposable
    private bool _disposed;

    ~BaseWriteOnlyRepository() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed) return;

        if(disposing) DbContext.Dispose();

        _disposed = true;
    }
    #endregion
}
