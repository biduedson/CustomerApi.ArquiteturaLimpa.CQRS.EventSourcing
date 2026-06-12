using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Account.Commands.Emails.Change;
using CustomerApi.Application.Account.Handlers.Emails.Change;
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

namespace CustomerApi.UnitTests.Application.Account.Handlers.Emails.Change;

[UnitTest]
public class ChangeEmailCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private const string DuplicateEmailMessage = "O endereço de e-mail informado já está em uso.";
    private const string RefreshTokenHash = "refresh-token-hash";
    private readonly ChangeEmailCommandValidator _validator = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task ChangeEmail_ValidCommand_ShouldReturnSuccessResult()
    {
        var user = CreateUser();

        _userRepository.Add(user);
        _userSessionRepository.Add(CreateUserSession(user.Id));

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new ChangeEmailCommand
        {
            UserId = user.Id,
            Email = CreateUniqueEmail("changed-account-email")
        };

        var handler = CreateChangeEmailCommandHandler();

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeEmail_DuplicateEmailCommand_ShouldReturnFailResult()
    {
        var users = CreateUsers(2);

        foreach (var user in users)
        {
            _userRepository.Add(user);
        }

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var command = new ChangeEmailCommand
        {
            UserId = users[0].Id,
            Email = users[1].Email.Address
        };

        var handler = CreateChangeEmailCommandHandler();

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(DuplicateEmailMessage);
    }

    [Fact]
    public async Task ChangeEmail_InvalidCommand_ShouldReturnFailResult()
    {
        var command = new ChangeEmailCommand();

        var handler = CreateChangeEmailCommandHandler();

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ChangeEmail_NotFoundUser_ShouldReturnNotFound()
    {
        var command = new ChangeEmailCommand
        {
            UserId = Guid.NewGuid(),
            Email = CreateUniqueEmail("not-found-account-email")
        };

        var handler = CreateChangeEmailCommandHandler();

        var act = await handler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.NotFound);
        act.Errors.Should()
            .NotBeNullOrEmpty()
            .And.Contain($"Nenhum usuario encontrado com o Id: {command.UserId}");
    }

    private ChangeEmailCommandHandler CreateChangeEmailCommandHandler() => new(
        _validator,
        _userRepository,
        _userSessionRepository,
        _unitOfWork);

    private static User CreateUser() => new Faker<User>()
        .CustomInstantiator(faker => User.Create(
            CreateUniqueUserName("account-email-user"),
            CreateUniqueEmail("current-account-email"),
            faker.PickRandom<UserRole>(),
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            "Gerente",
            "password-hash"))
        .Generate();

    private static List<User> CreateUsers(int count) => new Faker<User>()
        .CustomInstantiator(faker => User.Create(
            CreateUniqueUserName("account-email-conflict-user"),
            CreateUniqueEmail("account-email-conflict"),
            faker.PickRandom<UserRole>(),
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            "Gerente",
            "password-hash"))
        .Generate(count);

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
