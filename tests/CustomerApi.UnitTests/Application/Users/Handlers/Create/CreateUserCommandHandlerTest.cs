using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Users.Commands.Create;
using CustomerApi.Application.Users.Handlers.Create;
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

namespace CustomerApi.UnitTests.Application.Users.Handlers.Create;

[UnitTest]
public class CreateUserCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private const string ValidPassword = "Bidu1981@";
    private const string PasswordHash = "password-hash";
    private const string DuplicateUserNameMessage = "O Username informado esta indisponível.";
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";
    private readonly CreateUserCommandValidator _validator = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Create_ValidCommand_ShouldReturnCreatedResult()
    {
        var command = CreateValidCommand();
        var handler = CreateUserCommandHandler(_unitOfWork);

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsCreated().Should().BeTrue();
        act.Value.Should().NotBeNull();
        act.Value.Id.Should().NotBe(Guid.Empty);

        _passwordHasher.Received(1).Hash(ValidPassword);
    }

    [Fact]
    public async Task Create_DuplicateUserNameCommand_ShouldReturnFailResult()
    {
        var command = CreateValidCommand();
        var user = CreateUser(command.Username, CreateUniqueEmail("existing-user-with-different-email"));

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = CreateUserCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(DuplicateUserNameMessage);
    }

    [Fact]
    public async Task Create_DuplicateEmailCommand_ShouldReturnFailResult()
    {
        var command = CreateValidCommand();
        var user = CreateUser(CreateUniqueUserName("existing-user-with-different-username"), command.Email);

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = CreateUserCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(DuplicateEmailMessage);
    }

    [Fact]
    public async Task Create_InvalidCommand_ShouldReturnFailResult()
    {
        var command = new CreateUserCommand();
        var handler = CreateUserCommandHandler(Substitute.For<IUnitOfWork>());

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    private CreateUserCommandHandler CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _passwordHasher.Hash(ValidPassword).Returns(PasswordHash);

        return new CreateUserCommandHandler(
            _validator,
            _userRepository,
            _passwordHasher,
            unitOfWork);
    }

    private static CreateUserCommand CreateValidCommand() => new()
    {
        Username = CreateUniqueUserName("new-user"),
        Email = CreateUniqueEmail("new-user"),
        Role = UserRole.Operator,
        FullName = "John Doe",
        DateOfBirth = new DateTime(1990, 1, 1),
        JobTitle = "Developer",
        Password = ValidPassword
    };

    private static User CreateUser(string userName, string email) =>
        User.Create(
            userName,
            email,
            UserRole.Operator,
            "John Doe",
            new DateTime(1990, 1, 1),
            "Developer",
            PasswordHash);

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
