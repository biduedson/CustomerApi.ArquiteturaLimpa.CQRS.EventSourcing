using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CustomerApi.Core.Extensions;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate.Events;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Application.Customer.Queries;
using CustomerApi.Query.QueriesModel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerApi.Query.EventHandlers;

public class CustomerEventHandler(
    IMapper mapper,
    ISynchronizeDb synchronizeDb,
    ICacheService cacheService,
    ILogger<CustomerEventHandler> logger
) :
 INotificationHandler<CustomerCreatedEvent>,
 INotificationHandler<CustomerUpdatedEvent>,
 INotificationHandler<CustomerDeletedEvent>
{
    public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var customerQueryModel = mapper.Map<CustomerQueryModel>(notification);
        await synchronizeDb.UpsertAsync(customerQueryModel, filter => filter.Id == customerQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(CustomerDeletedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        await synchronizeDb.DeleteAsync<CustomerQueryModel>(filter => filter.Email == notification.Email);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(CustomerUpdatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var customerQueryModel = mapper.Map<CustomerQueryModel>(notification);
        await synchronizeDb.UpsertAsync(customerQueryModel, filter => filter.Id == customerQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    private async Task ClearCacheAsync(CustomerBaseEvent @event)
    {
        var cacheKeys = new[] { nameof(GetAllCustomerQuery), $"{nameof(GetCustomerByIdQuery)}_{@event.Id}" };
        await cacheService.RemoveAsync(cacheKeys);
    }
    private void LogEvent<TEvent>(TEvent @event) where TEvent : CustomerBaseEvent =>
    logger.LogInformation("----- Evento disparado {EventName}, modelo: {EventModel}", typeof(TEvent).Name, @event.ToJson());
}
