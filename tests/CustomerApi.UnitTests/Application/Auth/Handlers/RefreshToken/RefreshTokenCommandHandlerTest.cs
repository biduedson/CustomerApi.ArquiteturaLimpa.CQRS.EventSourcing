using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Bogus;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.RefreshToken;
using CustomerApi.Application.Auth.Handlers.RefreshToken;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
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
    private const string ValidPassword = "ValidTestPassword1!";
    private readonly RefreshTokenCommandValidator _validator = new();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly UserWriteOnlyRepository _userRepository = new(fixture.Context);
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task RefreshToken_ValidCommand_ShouldReturnSuccessResult()
    {
        // Prepara o cenario.
        var user = CreateDefaultUser();

        // Executa a acao.
        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        _userRepository.Add(user);
        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);
        _refreshTokenService.GenerateToken().Returns(NewRefreshToken);
        _refreshTokenService.HashToken(NewRefreshToken).Returns(NewRefreshTokenHash);
        _jwtTokenGenerator.GenerateAccessToken(Arg.Any<User>()).Returns(AccessToken);

        var validRefreshTokenCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentDesktop,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        var act = await handler.Handle(
            validRefreshTokenCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
        act.Value.AccessToken.Should().Be(AccessToken);
        act.Value.RefreshToken.Should().Be(NewRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidCommand_ShouldReturnFailResult()
    {
        // Prepara o cenario.
        var invalidRefreshTokenCommand = new RefreshTokenCommand();

        var handler = CreateRefreshTokenCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            invalidRefreshTokenCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task RefreshToken_RevokedSessionReuse_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.

        // Executa a acao.
        var userSession = UserSession.Create(
            Guid.NewGuid(),
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        userSession.Revoke("Revogado para o teste");

        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var refreshTokenWithRevokedSessionCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentDesktop,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        var act = await handler.Handle(
            refreshTokenWithRevokedSessionCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_SessionNotFound_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var refreshTokenWithNotFoundSessionCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentDesktop,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        // Executa a acao.
        var act = await handler.Handle(
            refreshTokenWithNotFoundSessionCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_DeviceChangeDetected_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        var user = CreateDefaultUser();

        // Executa a acao.
        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        _userRepository.Add(user);
        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var refreshTokenWithDifferentDeviceCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentMobile,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        var act = await handler.Handle(
            refreshTokenWithDifferentDeviceCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_UserNotFound_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        // Executa a acao.
        var userSession = UserSession.Create(
          Guid.NewGuid(),
          RefreshTokenHash,
          UserAgentDesktop,
          IpAddress,
          DateTime.UtcNow.AddDays(7));

        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var refreshTokenWithNotFoundUserCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentDesktop,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        var act = await handler.Handle(
            refreshTokenWithNotFoundUserCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);

    }

    [Fact]
    public async Task RefreshToken_InactiveUser_ShouldReturnUnauthorized()
    {
        // Prepara o cenario.
        var user = CreateDefaultUser();

        // Executa a acao.
        var userSession = UserSession.Create(
            user.Id,
            RefreshTokenHash,
            UserAgentDesktop,
            IpAddress,
            DateTime.UtcNow.AddDays(7));

        user.Deactivate();

        _userRepository.Add(user);
        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var refreshTokenWithInactiveUserCommand = new RefreshTokenCommand
        {
            RefreshToken = RefreshToken,
            UserAgent = UserAgentDesktop,
            IpAddress = IpAddress
        };

        var handler = CreateRefreshTokenCommandHandler();

        var act = await handler.Handle(
            refreshTokenWithInactiveUserCommand,
            CancellationToken.None);

        // Valida o resultado.
        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.Status.Should().Be(ResultStatus.Unauthorized);

    }

    #region Helpers

    private RefreshTokenCommandHandler CreateRefreshTokenCommandHandler() => new(
        _validator,
        _userRepository,
        _userSessionRepository,
        _jwtTokenGenerator,
        _refreshTokenService,
        TestJwtOptions.Create(),
        _unitOfWork);

    private static User CreateDefaultUser() =>
        new Faker<User>()
            .CustomInstantiator(faker => User.Create(
                faker.Person.UserName,
                faker.Person.Email,
                faker.PickRandom<UserRole>(),
                faker.Person.FullName,
                faker.Person.DateOfBirth,
                "Gerente",
                new BCryptPasswordHasher().Hash(ValidPassword)))
            .Generate();
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
