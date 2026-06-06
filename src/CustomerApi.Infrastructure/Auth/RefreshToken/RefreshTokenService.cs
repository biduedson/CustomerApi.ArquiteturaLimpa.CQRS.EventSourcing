using System;
using System.Security.Cryptography;
using System.Text;
using CustomerApi.Application.Abstractions.Auth;

namespace CustomerApi.Infrastructure.Auth.RefreshToken;

internal sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
