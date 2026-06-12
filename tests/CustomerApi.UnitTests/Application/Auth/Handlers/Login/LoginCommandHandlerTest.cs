using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.Application.Auth.Handlers.Login;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Auth.Password;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using CustomerApi.UnitTests.Helpers;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Auth.Handlers.Login;

[UnitTest]
public class LoginCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string ValidPassword = "ValidTestPassword1!";
    private const string SavedPassword = "SavedTestPassword1!";
    private const string WrongPassword = "WrongTestPassword1!";
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";
    private const string RefreshTokenHash = "refresh-token-hash";
    private const string IpAddress = "192.168.1.100";
    private const string UserAgent = "Edge 126 / Windows 10 / Desktop";
    private const string InvalidCredentialsMessage = "Credenciais inválidas.";
    private readonly LoginCommandValidator _validator = new();
    private readonly BCryptPasswordHasher _passwordHasher = new();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();

    [Fact]
    public async Task Login_ValidCommand_ShouldReturnSuccessResult()
    {
        var user = CreateDefaultUser(ValidPassword);

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _jwtTokenGenerator.GenerateAccessToken(Arg.Any<User>()).Returns(AccessToken);
        _refreshTokenService.GenerateToken().Returns(RefreshToken);
        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var validLoginCommand = new LoginCommand
        {
            Email = user.Email.Address,
            Password = ValidPassword,
            UserAgent = UserAgent,
            IpAddress = IpAddress
        };

        var handler = CreateLoginCommandHandler();

        var act = await handler.Handle(
            validLoginCommand,
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.Value.AccessToken.Should().Be(AccessToken);
        act.Value.RefreshToken.Should().Be(RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCommand_ShouldReturnFailResult()
    {
        var invalidLoginCommand = new LoginCommand();

        var handler = CreateLoginCommandHandler();

        var act = await handler.Handle(invalidLoginCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Login_UserNotFound_ShouldReturnUnauthorizedResult()
    {
        var loginWithNotFoundUserCommand = new LoginCommand
        {
            Email = "test@nonexistent.com",
            Password = SavedPassword,
            UserAgent = UserAgent,
            IpAddress = IpAddress
        };

        var handler = CreateLoginCommandHandler();

        var act = await handler.Handle(
            loginWithNotFoundUserCommand,
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Login_UserInactive_ShouldReturnUnauthorizedResult()
    {
        var user = CreateDefaultUser(SavedPassword);
        user.Deactivate();

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var loginWithInactiveUserCommand = new LoginCommand
        {
            Email = user.Email.Address,
            Password = SavedPassword,
            UserAgent = UserAgent,
            IpAddress = IpAddress
        };

        var handler = CreateLoginCommandHandler();

        var act = await handler.Handle(
            loginWithInactiveUserCommand,
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Login_PasswordError_ShouldReturnUnauthorizedResult()
    {
        var user = CreateDefaultUser(SavedPassword);

        _userRepository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var loginWithWrongPasswordCommand = new LoginCommand
        {
            Email = user.Email.Address,
            Password = WrongPassword,
            UserAgent = UserAgent,
            IpAddress = IpAddress
        };

        var handler = CreateLoginCommandHandler();

        var act = await handler.Handle(
            loginWithWrongPasswordCommand,
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    #region Helpers

    private LoginCommandHandler CreateLoginCommandHandler() => new(
        _validator,
        _userRepository,
        _userSessionRepository,
        _passwordHasher,
        _jwtTokenGenerator,
        _refreshTokenService,
        TestJwtOptions.Create(),
        _unitOfWork);

    private User CreateDefaultUser(string password) => new Faker<User>()
        .CustomInstantiator(faker => User.Create(
            faker.Person.UserName,
            faker.Person.Email,
            faker.PickRandom<UserRole>(),
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            "Gerente",
            _passwordHasher.Hash(password)))
        .Generate();

    #endregion
}
