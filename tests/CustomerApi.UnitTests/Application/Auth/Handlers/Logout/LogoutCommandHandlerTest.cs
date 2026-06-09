using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Logout;
using CustomerApi.Application.Auth.Handlers.Logout;
using CustomerApi.Core.SharedKernel;
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
    private readonly UserSessionWriteOnlyRepository _userSessionRepository = new(fixture.Context);
    private readonly UnitOfWork _unitOfWork = new(
        fixture.Context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());

    [Fact]
    public async Task Logout_ValidCommand_ShouldReturnSuccessResult()
    {
        var userSession = UserSession.Create(
           Guid.NewGuid(),
           RefreshTokenHash,
           UserAgent,
           IpAddress,
           DateTime.UtcNow.AddDays(7));

        _userSessionRepository.Add(userSession);

        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        _refreshTokenService.HashToken(RefreshToken).Returns(RefreshTokenHash);

        var validLogoutCommand = new LogoutCommand
        {
            RefreshToken = RefreshToken
        };

        var handler = CreateLogoutCommandHandler();

        var act = await handler.Handle(validLogoutCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_InvalidCommand_ShouldReturnFailResult()
    {
        var invalidLogoutCommand = new LogoutCommand();

        var handler = CreateLogoutCommandHandler();

        var act = await handler.Handle(invalidLogoutCommand, CancellationToken.None);

        act.Should().NotBeNull();
        act.IsSuccess.Should().BeFalse();
        act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    private LogoutCommandHandler CreateLogoutCommandHandler() =>
         new(
            _validator,
            _userSessionRepository,
            _refreshTokenService,
            _unitOfWork);
}
