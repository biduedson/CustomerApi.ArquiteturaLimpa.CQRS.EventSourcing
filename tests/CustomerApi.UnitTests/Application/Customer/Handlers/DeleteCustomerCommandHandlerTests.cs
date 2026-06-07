using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Application.Customer.Handlers;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Customer.Handlers;

[UnitTest]
public class DeleteCustomerCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string SuccessMessage = "Customer removido com sucesso!";

    private readonly DeleteCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Delete_ValidCustomerId_ShouldReturnsSuccessResult()
    {
        var customer = await PersistCustomerAsync(CreateCustomer());
        var command = new DeleteCustomerCommand(customer.Id);

        var act = await CreateHandler(CreateUnitOfWork()).Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be(SuccessMessage);
    }

    [Fact]
    public async Task Delete_InvalidCustomerId_ShouldReturnsFailureResult()
    {
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        var act = await CreateHandler(Substitute.For<IUnitOfWork>())
            .Handle(command, CancellationToken.None);

        AssertError(act, $"Nenhum cliente encontrado com o Id: {command.Id}");
    }

    [Fact]
    public async Task Delete_InvalidCommand_ShouldReturnsFailResult()
    {
        var act = await CreateHandler(Substitute.For<IUnitOfWork>())
            .Handle(new DeleteCustomerCommand(Guid.Empty), CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
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

    private UnitOfWork CreateUnitOfWork() => new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    private DeleteCustomerCommandHandler CreateHandler(IUnitOfWork unitOfWork) => new(
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

    private async Task<CustomerApi.Domain.Entities.CustomerAggregate.Customer> PersistCustomerAsync(
        CustomerApi.Domain.Entities.CustomerAggregate.Customer customer)
    {
        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        repository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        return customer;
    }

    #endregion
}
