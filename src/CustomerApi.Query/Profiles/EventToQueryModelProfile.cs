using AutoMapper;
using CustomerApi.Domain.Entities.CustomerAggregate.Events;
using CustomerApi.Domain.Entities.UserAggregate.Events;
using CustomerApi.Query.QueriesModel;

namespace CustomerApi.Query.Profiles;

public class EventToQueryModelProfile : Profile
{
  public EventToQueryModelProfile()
  {
    CreateMap<CustomerCreatedEvent, CustomerQueryModel>(MemberList.Destination)
       .ConstructUsing(@event => CreateCustomerQueryModel(@event));

    CreateMap<CustomerUpdatedEvent, CustomerQueryModel>(MemberList.Destination)
      .ConstructUsing(@event => CreateCustomerQueryModel(@event));

    CreateMap<CustomerDeletedEvent, CustomerQueryModel>(MemberList.Destination)
      .ConstructUsing(@event => CreateCustomerQueryModel(@event));


    CreateMap<UserCreatedEvent, UserQueryModel>(MemberList.Destination)
      .ConstructUsing(@event => CreateUserQueryModel(@event));

    CreateMap<UserUpdatedEvent, UserQueryModel>(MemberList.Destination)
      .ConstructUsing(@event => CreateUserQueryModel(@event));

    CreateMap<UserDeletedEvent, UserQueryModel>(MemberList.Destination)
      .ConstructUsing(@event => CreateUserQueryModel(@event));
  }

  public override string ProfileName => nameof(EventToQueryModelProfile);

  private static CustomerQueryModel CreateCustomerQueryModel<TEvent>(TEvent @event) where TEvent : CustomerBaseEvent =>
     new(@event.Id, @event.FirstName, @event.LastName, @event.Gender.ToString(), @event.Email, @event.DateOfBirth);

  private static UserQueryModel CreateUserQueryModel<TEvent>(TEvent @event) where TEvent : UserBaseEvent =>
     new(@event.Id, @event.UserName, @event.Email, @event.Role.ToString(), @event.FullName, @event.DateOfBirth, @event.JobTitle, @event.IsActive);
}
