using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Domain.Entities.UserSessionAggregate;

public interface IUserSessionWriteOnlyRepository : IWriteOnlyRepository<UserSession, Guid>
{
    Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash);
    Task<List<UserSession>> GetActiveByUserIdAsync(Guid userId);
    Task RevokeAllByUserIdAsync(Guid userId, string reason);
}
