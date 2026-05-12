
using Bogus;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.Entities.CustomerAggregate.Events;
using CustomerApi.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Domain.Entities.CustomerAggregate;

[UnitTest]
public class CustomerTests
{
    [Fact]
    public void Should_CustomerCreatedEvent_WhenCreate()
    {
        var customerFaker = new Faker<Customer>()
            .CustomInstantiator(faker => Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth
                ));

        var act = customerFaker.Generate();

        act.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerCreatedEvent>();
    }

    [Fact]
    public void Should_CustomerUpdatedEvent_WhenChangeEmail()
    {
        var customerEntity = new Faker<Customer>()
            .CustomInstantiator(faker => Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth
                )).Generate();

        var email = new Faker<Email>()
            .CustomInstantiator(faker => Email.Create(faker.Person.Email))
            .Generate();

        customerEntity.ChangeEmail(email);

        customerEntity.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerUpdatedEvent>();
    }

    [Fact]
    public void Should_CustomerDeleteEvent_WhenDelete()
    {
        var customerEntity = new Faker<Customer>()
            .CustomInstantiator(faker => Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth
                )).Generate();

        customerEntity.Delete();

        customerEntity.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerDeletedEvent>();
    }
}
