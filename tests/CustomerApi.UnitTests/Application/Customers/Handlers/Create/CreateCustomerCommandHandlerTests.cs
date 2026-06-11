using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CustomerApi.Application.Customers.Commands.Create;
using CustomerApi.Application.Customers.Handlers.Create;
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

namespace CustomerApi.UnitTests.Application.Customers.Handlers.Create;

[UnitTest]
public class CreateCustomerCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";
    private readonly CreateCustomerCommandValidator _validator = new();
    private readonly CustomerWriteOnlyRepository _customerRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Add_ValidCommand_ShouldCreateResult()
    {
        var validCreateCustomerCommand = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Gender = EGender.Male,
            Email = $"john.doe.{Guid.NewGuid():N}@test.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var handler = CreateCustomerCommandHandler(_unitOfWork);

        var act = await handler.Handle(validCreateCustomerCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsCreated().Should().BeTrue();
        act.Value.Should().NotBeNull();
        act.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Add_DuplicateEmailCommand_ShouldReturnFailResult()
    {
        var createCustomerWithDuplicateEmailCommand = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Gender = EGender.Male,
            Email = $"john.doe.{Guid.NewGuid():N}@test.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var customer = CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
            createCustomerWithDuplicateEmailCommand.FirstName,
            createCustomerWithDuplicateEmailCommand.LastName,
            createCustomerWithDuplicateEmailCommand.Gender,
            createCustomerWithDuplicateEmailCommand.Email,
            createCustomerWithDuplicateEmailCommand.DateOfBirth);

        _customerRepository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = CreateCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(createCustomerWithDuplicateEmailCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(DuplicateEmailMessage);
    }

    [Fact]
    public async Task Add_InvalidCommand_ShouldReturnFailResult()
    {
        var invalidCreateCustomerCommand = new CreateCustomerCommand();

        var handler = CreateCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(invalidCreateCustomerCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    #region Helpers

    private CreateCustomerCommandHandler CreateCustomerCommandHandler(IUnitOfWork unitOfWork) => new(
        _validator,
        _customerRepository,
        unitOfWork);

    #endregion
}
