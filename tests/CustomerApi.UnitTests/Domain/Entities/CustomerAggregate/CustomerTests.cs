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
        // Prepara o cenario.
        // Executa a acao.
        var act = CreateCustomer();

        // Valida o resultado.
        act.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerCreatedEvent>();
    }

    [Fact]
    public void Should_CustomerUpdatedEvent_WhenChangeEmail()
    {
        // Prepara o cenario.
        var customer = CreateCustomer();

        // Executa a acao.
        customer.ChangeEmail(CreateEmail());

        // Valida o resultado.
        customer.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerUpdatedEvent>();
    }

    [Fact]
    public void Should_CustomerDeleteEvent_WhenDelete()
    {
        // Prepara o cenario.
        var customer = CreateCustomer();

        // Executa a acao.
        customer.Delete();

        // Valida o resultado.
        customer.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<CustomerDeletedEvent>();
    }

    #region Helpers

    private static Customer CreateCustomer() =>
        new Faker<Customer>()
            .CustomInstantiator(faker => Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth))
            .Generate();

    private static Email CreateEmail() =>
        new Faker<Email>()
            .CustomInstantiator(faker => Email.Create(faker.Person.Email))
            .Generate();

    #endregion
}
