using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Users.Commands.Update.Role;
using CustomerApi.Application.Users.Handlers.Update.Role;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Users.Handlers.Update.Role;

[UnitTest]
public class UpdateUserRoleCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private const string RefreshTokenHash = "refresh-token-hash";
    private readonly UpdateUserRoleCommandValidator _validator = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task UpdateRole_ValidCommand_ShouldReturnSuccessResult()
    {
        // Prepara o cenario.
        var user = CreateUser(UserRole.Viewer);

        _userRepository.Add(user);
        _userSessionRepository.Add(CreateUserSession(user.Id));

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new UpdateUserRoleCommand
        {
            Id = user.Id,
            Role = UserRole.Admin
        };

        var handler = CreateUpdateUserRoleCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRole_InvalidCommand_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var command = new UpdateUserRoleCommand();

        var handler = CreateUpdateUserRoleCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task UpdateRole_InvalidRole_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var command = new UpdateUserRoleCommand
        {
            Id = Guid.NewGuid(),
            Role = (UserRole)999
        };

        var handler = CreateUpdateUserRoleCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task UpdateRole_NotFoundUser_ShouldReturnNotFound()
    {
        // Prepara o cenario.
        var command = new UpdateUserRoleCommand
        {
            Id = Guid.NewGuid(),
            Role = UserRole.Operator
        };

        var handler = CreateUpdateUserRoleCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(command, CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.NotFound);
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.Contain($"Nenhum usuario encontrado com o Id: {command.Id}");
    }

    private UpdateUserRoleCommandHandler CreateUpdateUserRoleCommandHandler() => new(
        _validator,
        _userRepository,
        _userSessionRepository,
        _unitOfWork);

    private static User CreateUser(UserRole role) => new Faker<User>()
        .CustomInstantiator(faker => User.Create(
            CreateUniqueUserName("role-update-user"),
            CreateUniqueEmail("role-update-user"),
            role,
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

    private static UserSession CreateUserSession(Guid userId) => new Faker<UserSession>()
        .CustomInstantiator(faker => UserSession.Create(
            userId,
            RefreshTokenHash,
            faker.Internet.UserAgent(),
            faker.Internet.IpAddress().ToString(),
            DateTime.UtcNow.AddDays(7)))
        .Generate();

    public async Task InitializeAsync()
    {
        await fixture.Context.Database.EnsureDeletedAsync();
        await fixture.Context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
