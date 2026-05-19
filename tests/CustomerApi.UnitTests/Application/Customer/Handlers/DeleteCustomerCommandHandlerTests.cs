using System;
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
public class DeleteCustomerCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private readonly DeleteCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Delete_ValidCustomerId_ShouldReturnsSuccessResult()
    {
        var customer = new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
        .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
            faker.Person.FirstName,
            faker.Person.LastName,
            faker.PickRandom<EGender>(),
            faker.Person.Email,
            faker.Person.DateOfBirth
        ))
        .Generate();

        var repository = new CustomerWriteOnlyRepository(fixture.Context);
        repository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var unitOfWork = new UnitOfWork(
            fixture.Context,
            Substitute.For<IEventStoreRepository>(),
            Substitute.For<IMediator>(),
            Substitute.For<ILogger<UnitOfWork>>());

        await unitOfWork.SaveChangesAsync();

        var handler = new DeleteCustomerCommandHandler(
            _validator,
             new CustomerWriteOnlyRepository(fixture.Context),
            unitOfWork
        );
        var command = new DeleteCustomerCommand(customer.Id);
        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be("Customer removido com sucesso!");
    }

    [Fact]
    public async Task Delete_InvalidCustomerId_ShouldReturnsFailureResult()
    {
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        var handler = new DeleteCustomerCommandHandler(
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

    [Fact]
    public async Task Delete_InvalidCommand_ShouldReturnsFailResult()
    {
        var command = new DeleteCustomerCommandHandler(
          _validator,
          new CustomerWriteOnlyRepository(fixture.Context),
          Substitute.For<IUnitOfWork>());

        var act = await command.Handle(new DeleteCustomerCommand(Guid.Empty), CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

}