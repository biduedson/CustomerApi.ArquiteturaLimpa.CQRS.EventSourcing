using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Logout;
using CustomerApi.Application.Auth.Handlers.Logout;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.UnitTests.Fixtures;
using CustomerApi.UnitTests.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Application.Auth.Handlers.Logout;

[UnitTest]
public class LogoutCommandHandlerTest(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
{
    private const string RefreshToken = "refresh-token";
    private const string RefreshTokenHash = "refresh-token-hash";
    private const string IpAddress = "192.168.1.100";
    private const string UserAgent = "Edge 126 / Windows 10 / Desktop";
    private readonly LogoutCommandValidator _validator = new();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();

    [Fact]
    public async Task Logout_ValidCommand_ShouldReturnsSuccessResult()
    {
        var userSessionRepository = new UserSessionWriteOnlyRepository(fixture.Context);

        var userSession = UserSession.Create(
           Guid.NewGuid(),
           RefreshTokenHash,
           UserAgent,
           IpAddress,
           DateTime.UtcNow.AddDays(7));

        userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var command = new LogoutCommand { RefreshToken = RefreshToken };

        var logoutHandler = CreateLogoutHandler();

        var act = await logoutHandler.Handle(command, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_InValidCommand_ShouldReturnsFailResult()
    {
        var invalidCommand = new LogoutCommand();

        var logoutHandler = CreateLogoutHandler();

        var act = await logoutHandler.Handle(invalidCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    private LogoutCommandHandler CreateLogoutHandler() =>
         new(
            _validator,
            new UserSessionWriteOnlyRepository(fixture.Context),
            _refreshTokenService,
            TestUnitOfWorkFactory.Create(fixture.Context));
}
