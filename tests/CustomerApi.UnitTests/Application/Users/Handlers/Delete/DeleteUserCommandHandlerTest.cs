using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Application.Users.Commands.Delete;
using CustomerApi.Application.Users.Handlers.Delete;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Users.Handlers.Delete;

[UnitTest]
public class DeleteUserCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private readonly DeleteUserCommandValidator _validator = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Delete_ValidUserId_ShouldReturnSuccessResult()
    {
        // Prepara o cenario.
        var user = CreateUser();

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new DeleteUserCommand(user.Id);
        var handler = CreateDeleteUserCommandHandler(_unitOfWork);

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_InvalidUserId_ShouldReturnFailureResult()
    {
        // Prepara o cenario.
        var command = new DeleteUserCommand(Guid.NewGuid());
        var handler = CreateDeleteUserCommandHandler(Substitute.For<IUnitOfWork>());

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain($"Nenhum usuario encontrado com o Id: {command.Id}");
    }

    [Fact]
    public async Task Delete_InvalidCommand_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var command = new DeleteUserCommand(Guid.Empty);
        var handler = CreateDeleteUserCommandHandler(Substitute.For<IUnitOfWork>());

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    private DeleteUserCommandHandler CreateDeleteUserCommandHandler(IUnitOfWork unitOfWork) => new(
        _validator,
        _userRepository,
        unitOfWork);

    private static User CreateUser() => new Faker<User>()
        .CustomInstantiator(faker => User.Create(
            CreateUniqueUserName("user-to-delete"),
            CreateUniqueEmail("user-to-delete"),
            faker.PickRandom<UserRole>(),
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            "Gerente",
            "password-hash"))
        .Generate();

    // O teste cria usuarios no SQLite; se repetir UserName, o SaveChanges falha por causa do indice unico.
    private static string CreateUniqueUserName(string scenario) =>
        $"{scenario}.{Guid.NewGuid():N}";

    // O teste cria usuarios no SQLite; se repetir Email, o SaveChanges falha por causa do indice unico.
    private static string CreateUniqueEmail(string scenario) =>
        $"{scenario}.{Guid.NewGuid():N}@test.com";

    public async Task InitializeAsync()
    {
        await fixture.Context.Database.EnsureDeletedAsync();
        await fixture.Context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
