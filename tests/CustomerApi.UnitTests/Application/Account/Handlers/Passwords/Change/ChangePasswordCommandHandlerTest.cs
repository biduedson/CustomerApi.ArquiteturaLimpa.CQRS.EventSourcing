using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Account.Commands.Passwords.Change;
using CustomerApi.Application.Account.Handlers.Passwords.Change;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Auth.Password;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Account.Handlers.Passwords.Change;

[UnitTest]
public class ChangePasswordCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private const string ValidPassword = "ValidTestPassword1!";
    private const string NewValidPassword = "NewValidTestPassword1!";
    private const string CurrentPassword = ValidPassword;
    private const string WrongCurrentPassword = "WrongPassword";
    private const string DifferentConfirmPassword = "DifferentTestPassword1!";
    private const string RefreshTokenHash = "refresh-token-hash";
    private readonly ChangePasswordCommandValidator _validator = new();
    private readonly BCryptPasswordHasher _passwordHasher = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task ChangePassword_ValidCommand_ShouldReturnSuccessResult()
    {
        // Prepara o cenario.
        var user = CreateDefaultUser();

        _userRepository.Add(user);

        var userSession = CreateDefaultUserSession(user.Id);

        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var validChangePasswordCommand = new ChangePasswordCommand
        {
            UserId = user.Id,
            CurrentPassword = CurrentPassword,
            NewPassword = NewValidPassword,
            ConfirmPassword = NewValidPassword
        };

        var handler = CreateChangePasswordCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            validChangePasswordCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_InvalidCommand_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var invalidChangePasswordCommand = new ChangePasswordCommand();

        var handler = CreateChangePasswordCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
           invalidChangePasswordCommand,
           CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ChangePassword_ConfirmPasswordDifferentFromNewPassword_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var changePasswordWithDifferentConfirmPasswordCommand = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = CurrentPassword,
            NewPassword = NewValidPassword,
            ConfirmPassword = DifferentConfirmPassword
        };

        var handler = CreateChangePasswordCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            changePasswordWithDifferentConfirmPasswordCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.Contain(error => error.ErrorMessage == "A confirmação da nova senha não corresponde à nova senha informada.");
    }

    [Fact]
    public async Task ChangePassword_NotFoundUser_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        var changePasswordWithNotFoundUserCommand = new ChangePasswordCommand
        {
            UserId = Guid.NewGuid(),
            CurrentPassword = CurrentPassword,
            NewPassword = NewValidPassword,
            ConfirmPassword = NewValidPassword
        };

        var handler = CreateChangePasswordCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            changePasswordWithNotFoundUserCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        var user = CreateDefaultUser();

        _userRepository.Add(user);

        var userSession = CreateDefaultUserSession(user.Id);

        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var changePasswordWithWrongCurrentPasswordCommand = new ChangePasswordCommand
        {
            UserId = user.Id,
            CurrentPassword = WrongCurrentPassword,
            NewPassword = NewValidPassword,
            ConfirmPassword = NewValidPassword
        };

        var handler = CreateChangePasswordCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            changePasswordWithWrongCurrentPasswordCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    private static UserSession CreateDefaultUserSession(Guid id) => new Faker<UserSession>()
            .CustomInstantiator(faker => UserSession.Create(
                 id,
                 RefreshTokenHash,
                 faker.Internet.UserAgent(),
                 faker.Internet.IpAddress().ToString(),
                 DateTime.UtcNow.AddDays(7)))
            .Generate();

    private User CreateDefaultUser() => new Faker<User>()
            .CustomInstantiator(faker => User.Create(
                 faker.Person.UserName,
                 faker.Person.Email,
                 faker.PickRandom<UserRole>(),
                 faker.Person.FullName,
                 faker.Person.DateOfBirth,
                 "Gerente",
                _passwordHasher.Hash(ValidPassword)))
            .Generate();

    private ChangePasswordCommandHandler CreateChangePasswordCommandHandler() => new(
        _validator,
        _userRepository,
        _userSessionRepository,
        _passwordHasher,
        _unitOfWork);

    // Recria o banco SQLite antes de cada teste para garantir isolamento e evitar dados de execuções anteriores.
    public async Task InitializeAsync()
    {
        await fixture.Context.Database.EnsureDeletedAsync();
        await fixture.Context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
