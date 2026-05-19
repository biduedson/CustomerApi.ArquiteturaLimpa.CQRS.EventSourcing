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
public class CreateCustomerCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Add_ValidCommand_ShouldCreateResult()
    {
        var command = new Faker<CreateCustomerCommand>()
            .RuleFor(command => command.FirstName, faker => faker.Person.FirstName)
            .RuleFor(command => command.LastName, faker => faker.Person.LastName)
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.Email, faker => faker.Person.Email)
            .RuleFor(command => command.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate();

        var unitOfWork = new UnitOfWork(
            fixture.Context,
            Substitute.For<IEventStoreRepository>(),
            Substitute.For<IMediator>(),
            Substitute.For<ILogger<UnitOfWork>>());

        var handler = new CreateCustomerCommandHandler(
            _validator,
            new CustomerWriteOnlyRepository(fixture.Context),
            unitOfWork
        );

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsCreated().Should().BeTrue();
        act.Value.Should().NotBeNull();
        act.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Add_DuplicateEmailCommand_ShouldReturnsFailResult()
    {
        var command = new Faker<CreateCustomerCommand>()
             .RuleFor(command => command.FirstName, faker => faker.Person.FirstName)
             .RuleFor(command => command.LastName, faker => faker.Person.LastName)
             .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
             .RuleFor(command => command.Email, faker => faker.Person.Email)
             .RuleFor(command => command.DateOfBirth, faker => faker.Person.DateOfBirth)
             .Generate();

        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        repository.Add(CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
          command.FirstName,
          command.LastName,
          command.Gender,
          command.Email,
          command.DateOfBirth));

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = new CreateCustomerCommandHandler(
           _validator,
          repository,
          Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
        .NotBeNullOrEmpty()
        .And.OnlyHaveUniqueItems()
        .And.Contain(errorMessage => errorMessage == "O endereço de e-mail informado já está em uso.");
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
}
