using System;
using System.Threading.Tasks;

namespace CustomerApi.Query.Abstractions;

public interface IReadOnlyRepository<TQueryModel, in TKey>
    where TQueryModel : IQueryModel<TKey>
    where TKey : IEquatable<TKey>
{
    Task<TQueryModel> GetByIdAsync(TKey id);
}