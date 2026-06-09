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
    private const string SuccessMessage = "Customer removido com sucesso!";
    private readonly DeleteCustomerCommandValidator _validator = new();
    private readonly CustomerWriteOnlyRepository _customerRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Delete_ValidCustomerId_ShouldReturnSuccessResult()
    {
        var customer = CreateCustomer();

        _customerRepository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var validDeleteCustomerCommand = new DeleteCustomerCommand(customer.Id);

        var handler = CreateDeleteCustomerCommandHandler(_unitOfWork);

        var act = await handler.Handle(validDeleteCustomerCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be(SuccessMessage);
    }

    [Fact]
    public async Task Delete_InvalidCustomerId_ShouldReturnFailureResult()
    {
        var deleteCustomerWithNotFoundCustomerCommand = new DeleteCustomerCommand(Guid.NewGuid());

        var handler = CreateDeleteCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(deleteCustomerWithNotFoundCustomerCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain($"Nenhum cliente encontrado com o Id: {deleteCustomerWithNotFoundCustomerCommand.Id}");
    }

    [Fact]
    public async Task Delete_InvalidCommand_ShouldReturnFailResult()
    {
        var invalidDeleteCustomerCommand = new DeleteCustomerCommand(Guid.Empty);

        var handler = CreateDeleteCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(invalidDeleteCustomerCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    #region Helpers

    private DeleteCustomerCommandHandler CreateDeleteCustomerCommandHandler(IUnitOfWork unitOfWork) => new(
        _validator,
        _customerRepository,
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

    #endregion
}
