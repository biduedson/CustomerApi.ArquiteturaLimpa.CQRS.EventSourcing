using System;
using Bogus;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Domain.Entities.UserSessionAggregate;

[UnitTest]
public class UserSessionTests
{
    [Fact]
    public void Should_ReturnsSuccess_WhenCreate()
    {
        var act = CreateUserSession();

        act.Should().NotBeNull();
        act.Should().BeOfType<UserSession>();
        act.IsActive.Should().BeTrue();
        act.IsExpired.Should().BeFalse();
        act.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Should_UpdateLastUsedAtAndIpAddress_WhenMarkAsUsed()
    {
        var userSession = CreateUserSession();
        var ipAddress = new Faker().Internet.IpAddress().ToString();

        userSession.MarkAsUsed(ipAddress);

        userSession.LastUsedAt.Should().NotBeNull();
        userSession.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void Should_ReturnsTrue_WhenFingerprintMatches()
    {
        var userSession = CreateUserSession();

        var act = userSession.MatchesFingerprint(userSession.UserAgent);

        act.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnsFalse_WhenFingerprintDoesNotMatch()
    {
        var userSession = CreateUserSession();

        var act = userSession.MatchesFingerprint("Different User-Agent");

        act.Should().BeFalse();
    }

    [Fact]
    public void Should_RevokeSession_WhenRevoke()
    {
        var userSession = CreateUserSession();
        var refreshTokenHash = userSession.RefreshTokenHash;
        var reason = "Revoked by test";
        var replacedByTokenHash = "new-refresh-token-hash";

        userSession.Revoke(reason, replacedByTokenHash);

        userSession.IsRevoked.Should().BeTrue();
        userSession.IsActive.Should().BeFalse();
        userSession.RevokedAt.Should().NotBeNull();
        userSession.RevocationReason.Should().Be(reason);
        userSession.ReplacedByTokenHash.Should().Be(replacedByTokenHash);
        userSession.RefreshTokenHash.Should().NotBe(refreshTokenHash);
        userSession.RefreshTokenHash.Should().StartWith("REVOKED_");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyUserId()
    {
        var act = () => UserSession.Create(
            Guid.Empty,
            "refresh-token-hash",
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O usuario da sessao deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyRefreshTokenHash()
    {
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            string.Empty,
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O hash do refresh token deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyUserAgent()
    {
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            "refresh-token-hash",
            string.Empty,
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O User-Agent da sessao deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithExpiredDate()
    {
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            "refresh-token-hash",
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(-1));

        act.Should().ThrowExactly<DomainException>()
            .WithMessage("A expiracao da sessao deve estar no futuro.");
    }

    #region Helpers

    private static UserSession CreateUserSession() =>
        new Faker<UserSession>()
            .CustomInstantiator(faker => UserSession.Create(
                Guid.NewGuid(),
                "refresh-token-hash",
                faker.Internet.UserAgent(),
                faker.Internet.IpAddress().ToString(),
                DateTime.UtcNow.AddDays(7)))
            .Generate();

    #endregion
}
