using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Application.Customer.Handlers;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using CustomerApi.UnitTests.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Customer.Handlers;

[UnitTest]
public class UpdateCustomerCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";
    private const string SuccessMessage = "Atualizado com sucesso!";

    private readonly UpdateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Update_ValidCommand_ShouldReturnsSuccessResult()
    {
        var customer = await PersistCustomerAsync(CreateCustomer());
        var command = CreateCommand(customer.Id);

        var act = await CreateHandler(TestUnitOfWorkFactory.Create(fixture.Context)).Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be(SuccessMessage);
    }

    [Fact]
    public async Task Update_DuplicateEmailCommand_ShouldReturnsFailResult()
    {
        var customers = await PersistCustomersAsync(CreateCustomers(2));
        var command = CreateCommand(customers[0].Id, customers[1].Email!.Address);

        var act = await CreateHandler(Substitute.For<IUnitOfWork>())
            .Handle(command, CancellationToken.None);

        AssertError(act, DuplicateEmailMessage);
    }

    [Fact]
    public async Task Update_NotFoundCustomer_ShouldReturnsFailResult()
    {
        var command = CreateCommand(Guid.NewGuid());

        var act = await CreateHandler(Substitute.For<IUnitOfWork>())
            .Handle(command, CancellationToken.None);

        AssertError(act, $"Nenhum cliente encontrado com o Id: {command.Id}");
    }

    #region Helpers

    private static void AssertError(Result act, string message)
    {
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(message);
    }

    private UpdateCustomerCommandHandler CreateHandler(IUnitOfWork unitOfWork) => new(
        _validator,
        new CustomerWriteOnlyRepository(fixture.Context),
        unitOfWork);

    private static CustomerApi.Domain.Entities.CustomerAggregate.Customer CreateCustomer() =>
        new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
            .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth))
            .Generate();

    private static List<CustomerApi.Domain.Entities.CustomerAggregate.Customer> CreateCustomers(int count) =>
        new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
            .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth))
            .Generate(count);

    private static UpdateCustomerCommand CreateCommand(Guid id, string? email = null) =>
        new Faker<UpdateCustomerCommand>()
            .RuleFor(command => command.Id, id)
            .RuleFor(command => command.Email, faker => email ?? faker.Person.Email.ToLowerInvariant())
            .Generate();

    private async Task<CustomerApi.Domain.Entities.CustomerAggregate.Customer> PersistCustomerAsync(
        CustomerApi.Domain.Entities.CustomerAggregate.Customer customer)
    {
        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        repository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        return customer;
    }

    private async Task<List<CustomerApi.Domain.Entities.CustomerAggregate.Customer>> PersistCustomersAsync(
        List<CustomerApi.Domain.Entities.CustomerAggregate.Customer> customers)
    {
        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        foreach (var customer in customers)
        {
            repository.Add(customer);
        }

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        return customers;
    }

    #endregion
}
