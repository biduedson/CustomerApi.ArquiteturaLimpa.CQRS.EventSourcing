using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Application.Customer.Handlers;
using CustomerApi.Application.Customer.Responses;
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
public class CreateCustomerCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";

    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Add_ValidCommand_ShouldCreateResult()
    {
        var command = CreateCustomerCommand();

        var act = await CreateHandler(CreateUnitOfWork()).Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsCreated().Should().BeTrue();
        act.Value.Should().NotBeNull();
        act.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Add_DuplicateEmailCommand_ShouldReturnsFailResult()
    {
        var command = CreateCustomerCommand();
        var customer = CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
            command.FirstName,
            command.LastName,
            command.Gender,
            command.Email,
            command.DateOfBirth);

        await PersistCustomerAsync(customer);

        var act = await CreateHandler(Substitute.For<IUnitOfWork>())
            .Handle(command, CancellationToken.None);

        AssertError(act, DuplicateEmailMessage);
    }

    [Fact]
    public async Task Add_InvalidCommand_ShouldReturnsFailResult()
    {
        var handler = new CreateCustomerCommandHandler(
            _validator,
            Substitute.For<ICustomerWriteOnlyRepository>(),
            Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(new CreateCustomerCommand(), CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    #region Helpers

    private static void AssertError(Result<CreatedCustomerResponse> act, string message)
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

    private CreateCustomerCommandHandler CreateHandler(IUnitOfWork unitOfWork) => new(
        _validator,
        new CustomerWriteOnlyRepository(fixture.Context),
        unitOfWork);

    private static CreateCustomerCommand CreateCustomerCommand() =>
        new Faker<CreateCustomerCommand>()
            .RuleFor(command => command.FirstName, faker => faker.Person.FirstName)
            .RuleFor(command => command.LastName, faker => faker.Person.LastName)
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.Email, faker => faker.Person.Email)
            .RuleFor(command => command.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate();

    private async Task PersistCustomerAsync(CustomerApi.Domain.Entities.CustomerAggregate.Customer customer)
    {
        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        repository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();
    }

    #endregion
}
