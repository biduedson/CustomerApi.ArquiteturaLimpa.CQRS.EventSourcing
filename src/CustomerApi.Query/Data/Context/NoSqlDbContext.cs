using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.QueriesModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace CustomerApi.Query.Data.Context;

public class NoSqlDbContext : IReadDbContext, ISynchronizeDb
{
    #region Construtor

    private const string DatabaseName = "CustomerApi";
    private const int RetryCount = 2;

    private static readonly ReplaceOptions DefaultReplaceOptions = new()
    {
        IsUpsert = true
    };

    private static readonly CreateIndexOptions DefaultCreateIndexOptions = new()
    {
        Unique = true,
        Sparse = true
    };

    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<NoSqlDbContext> _logger;
    private readonly AsyncRetryPolicy _mongoRetryPolicy;

    public NoSqlDbContext(IOptions<ConnectionOptions> options, ILogger<NoSqlDbContext> logger)
    {
        ConnectionString = options.Value.NoSqlConnection;

        _mongoClient = new MongoClient(options.Value.NoSqlConnection);
        _mongoDatabase = _mongoClient.GetDatabase(DatabaseName);
        _logger = logger;
        _mongoRetryPolicy = CreateRetriPolicy(logger);


    }
    #endregion

    #region IReadDbContext
    public string ConnectionString { get; }

    public IMongoCollection<TQueryModel> GetCollection<TQueryModel>() where TQueryModel : IQueryModel =>
        _mongoDatabase.GetCollection<TQueryModel>(typeof(TQueryModel).Name);

    public async Task CreateCollectionsAsync()
    {
        using var asyncCursor = await _mongoDatabase.ListCollectionNamesAsync();
        var collections = await asyncCursor.ToListAsync();

        foreach (var collectionName in GetCollectionNamesFromAssembly())
        {
            if (!collections.Exists(db => db.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                _logger.LogInformation("----- MongoDB: criando a coleção {Name}", collectionName);

                await _mongoDatabase.CreateCollectionAsync(collectionName);
            }
            else
            {
                _logger.LogInformation("----- MongoDB: coleção {Name} já existe", collectionName);
            }

        }

        await CreateIndexAsync();
    }
    private async Task CreateIndexAsync()
    {
        _logger.LogInformation("----- MongoDB: criando índices...");

        var customerIndexDefinition = Builders<CustomerQueryModel>.IndexKeys.Ascending(model => model.Email);
        var customerIndexModel = new CreateIndexModel<CustomerQueryModel>(customerIndexDefinition, DefaultCreateIndexOptions);
        var customerCollection = GetCollection<CustomerQueryModel>();

        await customerCollection.Indexes.CreateOneAsync(customerIndexModel);

        var userIndexDefinition = Builders<UserQueryModel>.IndexKeys.Ascending(model => model.Email);
        var userIndexModel = new CreateIndexModel<UserQueryModel>(userIndexDefinition, DefaultCreateIndexOptions);
        var userCollection = GetCollection<UserQueryModel>();

        await userCollection.Indexes.CreateOneAsync(userIndexModel);

        _logger.LogInformation("----- MongoDB: índices criados com sucesso");
    }
    private static List<string> GetCollectionNamesFromAssembly() =>
    [..Assembly
       .GetExecutingAssembly()
       .GetAllTypesOf<IQueryModel>()
       .Select(impl => impl.Name)
       .Distinct()];
    #endregion

    #region ISynchronizeDb

    public async Task UpsertAsync<TQueryModel>(TQueryModel queryModel, Expression<Func<TQueryModel, bool>> upsertFilter)
          where TQueryModel : IQueryModel
    {
        var collection = GetCollection<TQueryModel>();
        await _mongoRetryPolicy.ExecuteAsync(async () => await collection.ReplaceOneAsync(upsertFilter, queryModel, DefaultReplaceOptions));
    }

    public async Task DeleteAsync<TQueryModel>(Expression<Func<TQueryModel, bool>> deleteFilter)
        where TQueryModel : IQueryModel
    {
        var collection = GetCollection<TQueryModel>();
        await _mongoRetryPolicy.ExecuteAsync(async () => await collection.DeleteOneAsync(deleteFilter));
    }

    private static AsyncRetryPolicy CreateRetriPolicy(ILogger logger) =>
        Policy.Handle<MongoException>()
            .WaitAndRetryAsync(RetryCount, retryAttempt => SleepDurationProvider(retryAttempt, logger), (ex, _) => OnRetry(logger, ex));

    private static TimeSpan SleepDurationProvider(int retryAttempt, ILogger logger)
    {
        var sleepDuration = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
        logger.LogWarning("----- MongoDB: tentativa #{Count} com atraso {Delay}", retryAttempt, sleepDuration);
        return sleepDuration;
    }
    private static void OnRetry(ILogger logger, Exception ex) =>
    logger.LogError(ex, "Erro inesperado ao salvar no MongoDB: {Message}", ex.Message);
    #endregion

    #region IDisposable

    private bool _disposed;

    ~NoSqlDbContext() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _mongoClient.Dispose();
        }

        _disposed = true;
    }
    #endregion
}
