using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Application.Customers.Commands.Update;
using CustomerApi.Application.Customers.Handlers.Update;
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

namespace CustomerApi.UnitTests.Application.Customers.Handlers.Update;

[UnitTest]
public class UpdateCustomerCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";
    private const string SuccessMessage = "Atualizado com sucesso!";
    private readonly UpdateCustomerCommandValidator _validator = new();
    private readonly CustomerWriteOnlyRepository _customerRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Update_ValidCommand_ShouldReturnSuccessResult()
    {
        // Prepara o cenario.
        var customer = CreateCustomer();

        _customerRepository.Add(customer);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var validUpdateCustomerCommand = new UpdateCustomerCommand
        {
            Id = customer.Id,
            Email = $"updated.customer.{Guid.NewGuid():N}@test.com"
        };

        var handler = CreateUpdateCustomerCommandHandler(_unitOfWork);

        // Executa a acao.
        var act = await handler.Handle(validUpdateCustomerCommand, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.SuccessMessage.Should().Be(SuccessMessage);
    }

    [Fact]
    public async Task Update_DuplicateEmailCommand_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var customers = CreateCustomers(2);

        foreach (var customer in customers)
        {
            _customerRepository.Add(customer);
        }

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var updateCustomerWithDuplicateEmailCommand = new UpdateCustomerCommand
        {
            Id = customers[0].Id,
            Email = customers[1].Email!.Address
        };

        var handler = CreateUpdateCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        // Executa a acao.
        var act = await handler.Handle(updateCustomerWithDuplicateEmailCommand, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(DuplicateEmailMessage);
    }

    [Fact]
    public async Task Update_NotFoundCustomer_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var updateCustomerWithNotFoundCustomerCommand = new UpdateCustomerCommand
        {
            Id = Guid.NewGuid(),
            Email = $"updated.customer.{Guid.NewGuid():N}@test.com"
        };

        var handler = CreateUpdateCustomerCommandHandler(Substitute.For<IUnitOfWork>());

        // Executa a acao.
        var act = await handler.Handle(updateCustomerWithNotFoundCustomerCommand, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain($"Nenhum cliente encontrado com o Id: {updateCustomerWithNotFoundCustomerCommand.Id}");
    }

    #region Helpers

    private UpdateCustomerCommandHandler CreateUpdateCustomerCommandHandler(IUnitOfWork unitOfWork) => new(
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

    private static List<CustomerApi.Domain.Entities.CustomerAggregate.Customer> CreateCustomers(int count) =>
        new Faker<CustomerApi.Domain.Entities.CustomerAggregate.Customer>()
            .CustomInstantiator(faker => CustomerApi.Domain.Entities.CustomerAggregate.Customer.Create(
                faker.Person.FirstName,
                faker.Person.LastName,
                faker.PickRandom<EGender>(),
                faker.Person.Email,
                faker.Person.DateOfBirth))
            .Generate(count);

    #endregion
}
