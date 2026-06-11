using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Infrastructure.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Repositories;

internal sealed class UserSessionWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<UserSession, Guid>(dbContext), IUserSessionWriteOnlyRepository
{
    public Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash) =>
        DbContext
        .UserSessions
        .FirstOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash);

    public Task<UserSession?> GetByUserAgentAsync(string userAgent) =>
       DbContext
       .UserSessions
       .FirstOrDefaultAsync(session => session.UserAgent == userAgent);

    public Task<List<UserSession>> GetActiveByUserIdAsync(Guid userId) =>
        DbContext
        .UserSessions
        .Where(session =>
                 session.UserId == userId
                 && session.RevokedAt == null
                 && session.ExpiresAt > DateTime.UtcNow)
        .ToListAsync();

    public async Task RevokeAllByUserIdAsync(Guid userId, string reason)
    {
        var userSessions = await GetActiveByUserIdAsync(userId);

        foreach (var session in userSessions)
        {
            session.Revoke(reason);
        }

    }
}
