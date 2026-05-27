using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Core.Extensions;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerApi.Infrastructure.Data;

internal sealed class UnitOfWork(
    WriteDbContext writeDbContext,
    IEventStoreRepository eventStoreRepository,
    IMediator mediator,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task SaveChangesAsync()
    {
        var strategy = writeDbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {

            await using var transaction =
                await writeDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            logger.LogInformation("----- Iniciando transação: '{TransactionId}'", transaction.TransactionId);

            try
            {
                var (domainEvents, eventStores) = BeforeSaveChanges();

                var rowsAffected = await writeDbContext.SaveChangesAsync();

                logger.LogInformation("----- Commit da transação: '{TransactionId}'", transaction.TransactionId);

                await transaction.CommitAsync();


                await AfterSaveChangesAsync(domainEvents, eventStores);

                logger.LogInformation(
                    "----- Transação confirmada com sucesso: '{TransactionId}', Linhas afetadas: {RowsAffected}",
                    transaction.TransactionId,
                    rowsAffected);
            }
            catch (Exception ex)
            {
                logger.LogError(
                     ex,
                     "Ocorreu uma exceção inesperada ao confirmar a transação: '{TransactionId}', mensagem: {Message}",
                     transaction.TransactionId,
                     ex.Message);

                await transaction.RollbackAsync();

                throw;
            }
        });
    }
    private (IReadOnlyList<BaseEvent> domainEvents, IReadOnlyList<EventStore> eventStores) BeforeSaveChanges()
    {
        var domainEntities = writeDbContext
        .ChangeTracker
        .Entries<BaseEntity>()
        .Where(entry => entry.Entity.DomainEvents.Any())
        .ToList();

        var domainEvents = domainEntities
        .SelectMany(entry => entry.Entity.DomainEvents)
        .ToList();

        var eventsStore = domainEvents
        .ConvertAll(@event => new EventStore(@event.AggregateId, @event.GetGenericTypeName(), @event.ToJson()!));

        domainEntities.ForEach(entry => entry.Entity.ClearDomainEvents());
        return (domainEvents.AsReadOnly(), eventsStore.AsReadOnly());
    }
    private async Task AfterSaveChangesAsync(
        IReadOnlyList<BaseEvent> domainEvents,
        IReadOnlyList<EventStore> eventStores
        )
    {
        if (domainEvents.Count > 0)
            await Task.WhenAll(domainEvents.Select(@event => mediator.Publish(@event)));

        if (eventStores.Count > 0)
            await eventStoreRepository.StoreAsync(eventStores);
    }

    #region Disposable
    private bool _disposed;

    ~UnitOfWork() => Dispose(false);
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
            writeDbContext.Dispose();
            eventStoreRepository.Dispose();
        }

        _disposed = true;
    }
    #endregion
}
