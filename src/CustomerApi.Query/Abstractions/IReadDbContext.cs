using System;
using System.Threading.Tasks;
using MongoDB.Driver;


namespace CustomerApi.Query.Abstractions;

public interface IReadDbContext : IDisposable
{
    string ConnectionString { get; }

    IMongoCollection<TQueryModel> GetCollection<TQueryModel>() where TQueryModel : IQueryModel;

    Task CreateCollectionsAsync();
}
