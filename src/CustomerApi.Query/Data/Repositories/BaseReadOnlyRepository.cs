using System;
using System.Threading.Tasks;
using CustomerApi.Query.Abstractions;
using MongoDB.Driver;

namespace CustomerApi.Query.Data.Repositories;

public abstract class BaseReadOnlyRepository<TQueryModel, TKey>(IReadDbContext context) : IReadOnlyRepository<TQueryModel, TKey>
       where TQueryModel : IQueryModel<TKey>
       where TKey : IEquatable<TKey>
{
    protected readonly IMongoCollection<TQueryModel> Collection = context.GetCollection<TQueryModel>();

    public async Task<TQueryModel> GetByIdAsync(TKey id)
    {
        using var asyncCursor = await Collection.FindAsync(queryModel => queryModel.Id.Equals(id));
        return await asyncCursor.FirstOrDefaultAsync();
    }
}
