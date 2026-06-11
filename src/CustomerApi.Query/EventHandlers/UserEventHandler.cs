using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CustomerApi.Core.Extensions;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate.Events;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Application.User.Queries;
using CustomerApi.Query.QueriesModel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerApi.Query.EventHandlers;

public class UserEventHandler(
    IMapper mapper,
    ISynchronizeDb synchronizeDb,
    ICacheService cacheService,
    ILogger<UserEventHandler> logger
) :
 INotificationHandler<UserCreatedEvent>,
 INotificationHandler<UserUpdatedEvent>,
 INotificationHandler<UserDeletedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var userQueryModel = mapper.Map<UserQueryModel>(notification);
        await synchronizeDb.UpsertAsync(userQueryModel, filter => filter.Id == userQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        await synchronizeDb.DeleteAsync<UserQueryModel>(filter => filter.Email == notification.Email);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var userQueryModel = mapper.Map<UserQueryModel>(notification);
        await synchronizeDb.UpsertAsync(userQueryModel, filter => filter.Id == userQueryModel.Id);
        await ClearCacheAsync(notification);
    }

    private async Task ClearCacheAsync(UserBaseEvent @event)
    {
        var cacheKeys = new[] { nameof(GetAllUserQuery), $"{nameof(GetUserByIdQuery)}_{@event.Id}" };
        await cacheService.RemoveAsync(cacheKeys);
    }
    private void LogEvent<TEvent>(TEvent @event) where TEvent : UserBaseEvent =>
    logger.LogInformation("----- Evento disparado {EventName}, modelo: {EventModel}", typeof(TEvent).Name, @event.ToJson());


}
