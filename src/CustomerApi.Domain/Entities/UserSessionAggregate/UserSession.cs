using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Exceptions;

namespace CustomerApi.Domain.Entities.UserSessionAggregate;

public class UserSession : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? RevocationReason { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => RevokedAt == null && !IsExpired;

    private UserSession() { }

    private UserSession(
        Guid userId,
        string refreshTokenHash,
        string userAgent,
        string? ipAddress,
        DateTime expiresAt)
    {

        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        UserAgent = userAgent;
        IpAddress = ipAddress;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public static UserSession Create(
        Guid userId,
        string refreshTokenHash,
        string userAgent,
        string? ipAddress,
        DateTime expiresAt)
    {

        DomainException.ThrowIf(userId == Guid.Empty, "O usuario da sessao deve ser informado.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(refreshTokenHash), "O hash do refresh token deve ser informado.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(userAgent), "O User-Agent da sessao deve ser informado.");
        DomainException.ThrowIf(expiresAt <= DateTime.UtcNow, "A expiracao da sessao deve estar no futuro.");

        return new UserSession(userId, refreshTokenHash, userAgent, ipAddress, expiresAt);
    }
    public void MarkAsUsed(string? ipAddress)
    {
        LastUsedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
    }

    public bool MatchesFingerprint(string userAgent) =>
        string.Equals(UserAgent, userAgent, StringComparison.Ordinal);

    public void Revoke(string? reason, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }


}

