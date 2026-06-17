using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomerApi.BlazorUI.Services.Authentication;

internal static class AuthClaimsFactory
{
    public static List<Claim> CreateClaims(string? accessToken, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email)
        };

        if (string.IsNullOrWhiteSpace(accessToken))
            return claims;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        AddClaimIfExists(claims, jwt, ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);
        AddClaimIfExists(claims, jwt, ClaimTypes.Name, ClaimTypes.Name, JwtRegisteredClaimNames.Name);
        AddClaimIfExists(claims, jwt, ClaimTypes.Email, ClaimTypes.Email, JwtRegisteredClaimNames.Email);

        var roles = jwt.Claims
            .Where(claim =>
                claim.Type == ClaimTypes.Role ||
                claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    private static void AddClaimIfExists(
        List<Claim> claims,
        JwtSecurityToken jwt,
        string claimType,
        params string[] tokenClaimTypes)
    {
        var value = jwt.Claims
            .FirstOrDefault(claim => tokenClaimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            ?.Value;

        if (!string.IsNullOrWhiteSpace(value))
            claims.Add(new Claim(claimType, value));
    }
}
