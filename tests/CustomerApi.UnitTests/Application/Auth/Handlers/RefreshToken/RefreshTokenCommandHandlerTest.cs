using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.RefreshToken;
using CustomerApi.Application.Auth.Handlers.RefreshToken;
using CustomerApi.Core.AppSettings;
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
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Auth.Handlers.RefreshToken;

[UnitTest]
public class RefreshTokenCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>, IAsyncLifetime
{
    private const string AccessToken = "access-token";
    private const string RefreshToken = "refresh-token";
    private const string RefreshTokenHash = "refresh-token-hash";
    private const string NewRefreshToken = "new-refresh-token";
    private const string NewRefreshTokenHash = "new-refresh-token-hash";
    private const string IpAddress = "192.168.1.100";
    private const string UserAgentDesktop = "Edge 126 / Windows 10 / Desktop";
    private const string UserAgentMobile = "Opera 126 / Windows 10 / Iphone14";
    private readonly RefreshTokenCommandValidator _validator = new();
    private readonly BCryptPasswordHasher _passwordHasher = new();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();

    [Fact]
    public async Task RefreshToken_ValidCommand_ShouldReturnsSuccessResult()
    {
        var user = CreateFakerUser("testPassword");

        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        await PersistUserAndUserSessionAsync(user, userSession);

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);
        _refreshTokenService.GenerateToken().Returns(NewRefreshToken);
        _refreshTokenService.HashToken(NewRefreshToken).Returns(NewRefreshTokenHash);
        _jwtTokenGenerator.GenerateAccessToken(Arg.Any<User>()).Returns(AccessToken);

        var act = await CreateRefreshTokenHandler().Handle(
            CreateRefreshTokenCommand(),
            CancellationToken.None);

        await InitializeAsync();

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.Value.AccessToken.Should().Be(AccessToken);
        act.Value.RefreshToken.Should().Be(NewRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InValidCommand_ShouldReturnsFailResult()
    {
        var act = await CreateRefreshTokenHandler().Handle(
             new RefreshTokenCommand(),
             CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task RefreshToken_RevokedSessionReuse_ShouldReturnUnauthorized()
    {

        var userSession = UserSession.Create(
            Guid.NewGuid(),
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        userSession.Revoke("Revogado para o teste");

        await PersistUserSessionAsync(userSession);

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var act = await CreateRefreshTokenHandler().Handle(
            CreateRefreshTokenCommand(),
            CancellationToken.None);

        await InitializeAsync();

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_SessionNotFoud_ShouldReturnUnauthorized()
    {
        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var act = await CreateRefreshTokenHandler().Handle(
            CreateRefreshTokenCommand(),
            CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_DeviceChangeDetected_ShouldReturnUnauthorized()
    {
        var user = CreateFakerUser("testPassword");

        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        await PersistUserAndUserSessionAsync(user, userSession);

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var command = CreateRefreshTokenCommand();

        command.UserAgent = UserAgentMobile;

        var act = await CreateRefreshTokenHandler().Handle(
            command,
            CancellationToken.None);

        await InitializeAsync();

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_UserNotFoud_ShouldReturnUnauthorized()
    {
        var userSession = UserSession.Create(
          Guid.NewGuid(),
          RefreshTokenHash,
          UserAgentDesktop,
          IpAddress,
          DateTime.UtcNow.AddDays(7));

        await PersistUserSessionAsync(userSession);

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var act = await CreateRefreshTokenHandler().Handle(
            CreateRefreshTokenCommand(),
            CancellationToken.None);

        await InitializeAsync();

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);

    }

    [Fact]
    public async Task RefreshToken_InactiveUser_ShouldReturnUnauthorized()
    {
        var user = CreateFakerUser("testPassword");

        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        user.Deactivate();

        await PersistUserAndUserSessionAsync(user, userSession);

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var act = await CreateRefreshTokenHandler().Handle(
            CreateRefreshTokenCommand(),
            CancellationToken.None);

        await InitializeAsync();

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);

    }

    #region Helpers

    private UnitOfWork CreateUnitOfWork() => new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    private RefreshTokenCommandHandler CreateRefreshTokenHandler() => new(
        _validator,
        new UserWriteOnlyRepository(fixture.Context),
        new UserSessionWriteOnlyRepository(fixture.Context),
        _jwtTokenGenerator,
        _refreshTokenService,
        CreateJwtOptions(),
        CreateUnitOfWork());

    private async Task PersistUserAndUserSessionAsync(User user, UserSession userSession)
    {
        var userRepository = new UserWriteOnlyRepository(fixture.Context);
        var userSessionRepository = new UserSessionWriteOnlyRepository(fixture.Context);

        userRepository.Add(user);
        userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();
    }

    private async Task PersistUserSessionAsync(UserSession userSession)
    {
        var userSessionRepository = new UserSessionWriteOnlyRepository(fixture.Context);
        userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();
    }

    private User CreateFakerUser(string password, bool inactive = false)
    {
        var user = new Faker<User>()
            .CustomInstantiator(faker => User.Create(
                faker.Person.UserName,
                faker.Person.Email,
                faker.PickRandom<UserRole>(),
                faker.Person.FullName,
                faker.Person.DateOfBirth,
                "Gerente",
                _passwordHasher.Hash(password)))
            .Generate();

        if (inactive)
            user.Deactivate();

        return user;
    }

    private static RefreshTokenCommand CreateRefreshTokenCommand() =>
        new Faker<RefreshTokenCommand>()
            .RuleFor(command => command.RefreshToken, RefreshToken)
            .RuleFor(command => command.UserAgent, UserAgentDesktop)
            .RuleFor(command => command.IpAddress, faker => faker.Internet.Ip())
            .Generate();

    private static IOptions<JwtOptions> CreateJwtOptions()
    {
        var jwtOptions = new JwtOptions();

        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Issuer))!.SetValue(jwtOptions, "CustomerApi");
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Audience))!.SetValue(jwtOptions, "CustomerApi.Tests");
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Secret))!.SetValue(jwtOptions, "CHANGE_THIS_SECRET_TO_A_LONG_SECURE_KEY");
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.AccessTokenExpirationInMinutes))!.SetValue(jwtOptions, 15);
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.RefreshTokenExpirationInDays))!.SetValue(jwtOptions, 7);

        return Options.Create(jwtOptions);
    }
    #endregion

    #region IAsyncLifetime
    public async Task InitializeAsync()
    {
        await fixture.Context.Database.EnsureDeletedAsync();
        await fixture.Context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    #endregion

}
