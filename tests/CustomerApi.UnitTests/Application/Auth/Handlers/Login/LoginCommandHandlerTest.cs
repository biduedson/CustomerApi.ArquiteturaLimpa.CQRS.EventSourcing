using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.Application.Auth.Handlers.Login;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Auth.Password;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using CustomerApi.UnitTests.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Auth.Handlers.Login;

[UnitTest]
public class LoginCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string ValidPassword = "Bidu1981@";
    private const string SavedPassword = "passwordTest";
    private const string WrongPassword = "passwordErrorTest";
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";
    private const string RefreshTokenHash = "refresh-token-hash";
    private const string InvalidCredentialsMessage = "Credenciais inválidas.";

    private readonly LoginCommandValidator _validator = new();
    private readonly BCryptPasswordHasher _passwordHasher = new();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();

    [Fact]
    public async Task Login_ValidCommand_ShouldReturnsSuccessResult()
    {
        var user = await PersistUserAsync(UserTestBuilder.Create(ValidPassword));

        _jwtTokenGenerator.GenerateAccessToken(Arg.Any<User>()).Returns(AccessToken);
        _refreshTokenService.GenerateToken().Returns(RefreshToken);
        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var act = await CreateHandler().Handle(
            CreateCommand(user.Email.Address, ValidPassword),
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.Value.AccessToken.Should().Be(AccessToken);
        act.Value.RefreshToken.Should().Be(RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCommand_ShouldReturnsFailResult()
    {
        var act = await CreateHandler().Handle(new LoginCommand(), CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Login_UserNotFound_ShouldReturnsUnauthorizedResult()
    {
        var act = await CreateHandler().Handle(
            CreateCommand("test@nonexistent.com", SavedPassword),
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Login_UserInactive_ShouldReturnsUnauthorizedResult()
    {
        var user = await PersistUserAsync(UserTestBuilder.Create(SavedPassword, inactive: true));

        var act = await CreateHandler().Handle(
            CreateCommand(user.Email.Address, SavedPassword),
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Login_PasswordError_ShouldReturnsUnauthorizedResult()
    {
        var user = await PersistUserAsync(UserTestBuilder.Create(SavedPassword));

        var act = await CreateHandler().Handle(
            CreateCommand(user.Email.Address, WrongPassword),
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.IsUnauthorized().Should().BeTrue();
        act.Errors.Should().Contain(InvalidCredentialsMessage);
    }

    #region Helpers

    private LoginCommandHandler CreateHandler() => new(
        _validator,
        new UserWriteOnlyRepository(fixture.Context),
        new UserSessionWriteOnlyRepository(fixture.Context),
        _passwordHasher,
        _jwtTokenGenerator,
        _refreshTokenService,
        TestJwtOptions.Create(),
        TestUnitOfWorkFactory.Create(fixture.Context));

    private static LoginCommand CreateCommand(string email, string password) =>
        new Faker<LoginCommand>()
            .RuleFor(command => command.Email, email)
            .RuleFor(command => command.Password, password)
            .RuleFor(command => command.UserAgent, faker => faker.Internet.UserAgent())
            .RuleFor(command => command.IpAddress, faker => faker.Internet.Ip())
            .Generate();

    private async Task<User> PersistUserAsync(User user)
    {
        var repository = new UserWriteOnlyRepository(fixture.Context);

        repository.Add(user);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        return user;
    }

    #endregion
}
