using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Core.AppSettings;
using CustomerApi.Domain.Entities.UserAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerApi.Infrastructure.Auth;

internal sealed class JwtTokenGenerator(
    IOptions<JwtOptions> jwtOptions,
    ILogger<JwtTokenGenerator> logger
    ) : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly ILogger _logger = logger;

    public string GenerateAccessToken(User user)
    {
        try
        {
            _logger.LogInformation("Iniciando geração do access token para o usuário {UserId}.", user.Id);

            var claims = CreateClaims(user);

            var signingCredentials = CreateSigninCredentials();

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: signingCredentials
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation(
          "Access token gerado com sucesso para o usuário {UserId}. Expiração prevista para {ExpirationDateUtc}.",
          user.Id,
          expiresAt);

            return accessToken;

        }
        catch (Exception ex)
        {
            _logger.LogError(
           ex,
           "Ocorreu um erro ao gerar o access token para o usuário {UserId}.",
           user.Id);
            throw;
        }
    }

    private SigningCredentials CreateSigninCredentials()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
    private static Claim[] CreateClaims(User user)
    {
        return
        [
           new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
           new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
           new Claim(ClaimTypes.Name, user.UserName),
           new Claim(ClaimTypes.Email, user.Email.Address),
           new Claim(ClaimTypes.Role, user.Role.ToString()),
           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
           new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)];

    }
}