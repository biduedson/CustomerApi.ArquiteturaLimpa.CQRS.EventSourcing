using System.Threading;
using System.Threading.Tasks;
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
public class UpdateCustomerCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private readonly UpdateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Update_ValidCommand_ShouldReturnsSuccessResult()
    {

        var customer = new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
        .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
         faker.Person.FirstName,
         faker.Person.LastName,
         faker.PickRandom<EGender>(),
         faker.Person.Email,
         faker.Person.DateOfBirth
        )).Generate();

        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        repository.Add(customer);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var unitOfWork = new UnitOfWork(
         fixture.Context,
         Substitute.For<IEventStoreRepository>(),
         Substitute.For<IMediator>(),
         Substitute.For<ILogger<UnitOfWork>>());

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new Faker<UpdateCustomerCommand>()
        .RuleFor(command => command.Id, customer.Id)
        .RuleFor(command => command.Email, faker => faker.Person.Email.ToLowerInvariant());

        var handler = new UpdateCustomerCommandHandler(
            _validator,
             new CustomerWriteOnlyRepository(fixture.Context),
            unitOfWork);

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be("Atualizado com sucesso!");
    }

    [Fact]
    public async Task Update_DuplicateEmailCommand_ShouldReturnsFailResult()
    {
        var customers = new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
        .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
            faker.Person.FirstName,
            faker.Person.LastName,
            faker.PickRandom<EGender>(),
            faker.Person.Email,
            faker.Person.DateOfBirth
        )).Generate(2);

        var repository = new CustomerWriteOnlyRepository(fixture.Context);

        foreach (var customer in customers)
        {
            repository.Add(customer);
        }

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new Faker<UpdateCustomerCommand>()
        .RuleFor(command => command.Id, customers[0].Id)
        .RuleFor(command => command.Email, customers[1].Email!.Address);

        var handler = new UpdateCustomerCommandHandler(
            _validator,
            new CustomerWriteOnlyRepository(fixture.Context),
            Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
        .NotBeNullOrEmpty()
        .And.OnlyHaveUniqueItems()
        .And.Contain("O endereço de e-mail informado já está em uso.");
    }

    [Fact]
    public async Task Update_NotFoundCustomer_ShouldReturnsFailResult()
    {
        var command = new Faker<UpdateCustomerCommand>()
        .RuleFor(command => command.Id, faker => faker.Random.Guid())
        .RuleFor(command => command.Email, faker => faker.Person.Email)
        .Generate();

        var handler = new UpdateCustomerCommandHandler(
        _validator,
        new CustomerWriteOnlyRepository(fixture.Context),
        Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
        .NotBeNullOrEmpty()
        .And.OnlyHaveUniqueItems()
        .And.Contain(errorMessage => errorMessage == $"Nenhum cliente encontrado com o Id: {command.Id}");
    }
}