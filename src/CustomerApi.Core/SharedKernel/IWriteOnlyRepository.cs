namespace CustomerApi.Core.SharedKernel;

public interface IWriteOnlyRepository<TEntity, in TKey> : IDisposable
    where TEntity : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<TEntity> GetById(TKey id);
}
