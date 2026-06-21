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
        // Prepara o cenario.
        // Executa a acao.
        var act = CreateUserSession();

        // Valida o resultado.
        act.Should().NotBeNull();
        act.Should().BeOfType<UserSession>();
        act.IsActive.Should().BeTrue();
        act.IsExpired.Should().BeFalse();
        act.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Should_UpdateLastUsedAtAndIpAddress_WhenMarkAsUsed()
    {
        // Prepara o cenario.
        var userSession = CreateUserSession();
        var ipAddress = new Faker().Internet.IpAddress().ToString();

        // Executa a acao.
        userSession.MarkAsUsed(ipAddress);

        // Valida o resultado.
        userSession.LastUsedAt.Should().NotBeNull();
        userSession.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void Should_ReturnsTrue_WhenFingerprintMatches()
    {
        // Prepara o cenario.
        var userSession = CreateUserSession();

        // Executa a acao.
        var act = userSession.MatchesFingerprint(userSession.UserAgent);

        // Valida o resultado.
        act.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnsFalse_WhenFingerprintDoesNotMatch()
    {
        // Prepara o cenario.
        var userSession = CreateUserSession();

        // Executa a acao.
        var act = userSession.MatchesFingerprint("Different User-Agent");

        // Valida o resultado.
        act.Should().BeFalse();
    }

    [Fact]
    public void Should_RevokeSession_WhenRevoke()
    {
        // Prepara o cenario.
        var userSession = CreateUserSession();
        var refreshTokenHash = userSession.RefreshTokenHash;
        var reason = "Revoked by test";
        var replacedByTokenHash = "new-refresh-token-hash";

        // Executa a acao.
        userSession.Revoke(reason, replacedByTokenHash);

        // Valida o resultado.
        userSession.IsRevoked.Should().BeTrue();
        userSession.IsActive.Should().BeFalse();
        userSession.RevokedAt.Should().NotBeNull();
        userSession.RevocationReason.Should().Be(reason);
        userSession.ReplacedByTokenHash.Should().Be(replacedByTokenHash);
        userSession.RefreshTokenHash.Should().Be(refreshTokenHash);
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyUserId()
    {
        // Prepara o cenario.
        // Executa a acao.
        var act = () => UserSession.Create(
            Guid.Empty,
            "refresh-token-hash",
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        // Valida o resultado.
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O usuario da sessao deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyRefreshTokenHash()
    {
        // Prepara o cenario.
        // Executa a acao.
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            string.Empty,
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        // Valida o resultado.
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O hash do refresh token deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithEmptyUserAgent()
    {
        // Prepara o cenario.
        // Executa a acao.
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            "refresh-token-hash",
            string.Empty,
            "127.0.0.1",
            DateTime.UtcNow.AddDays(7));

        // Valida o resultado.
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O User-Agent da sessao deve ser informado.");
    }

    [Fact]
    public void Should_ReturnsFail_WhenCreateWithExpiredDate()
    {
        // Prepara o cenario.
        // Executa a acao.
        var act = () => UserSession.Create(
            Guid.NewGuid(),
            "refresh-token-hash",
            "Edge 126 / Windows 10 / Desktop",
            "127.0.0.1",
            DateTime.UtcNow.AddDays(-1));

        // Valida o resultado.
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
