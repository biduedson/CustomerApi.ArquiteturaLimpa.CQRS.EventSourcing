using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CustomerApi.Application.Auth.Responses;
using Microsoft.AspNetCore.Http;

namespace CustomerApi.WebApi.Extensions;

public static class AuthCookieExtensions
{
    private const string AccessTokenCookie = "access_Token";
    private const string RefreshTokenCookie = "refresh_Token";

    public static void AppendAuthCookies(this HttpResponse response, AuthenticationResponse authentication)
    {
        response.Cookies.Append(
            AccessTokenCookie,
            authentication.AccessToken,
            CreateCookieOptions(authentication.AccessTokenExpiresAt));

        response.Cookies.Append(
            RefreshTokenCookie,
            authentication.RefreshToken,
            CreateCookieOptions(authentication.RefreshTokenExpiresAt));
    }

    public static void DeleteAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookie, new CookieOptions { Path = "/api/auth" });
        response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/auth" });
    }

    public static string GetRefreshTokenCookies(this HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookie] ?? string.Empty;
    }

    public static string GetAccessTokenCookies(this HttpRequest request)
    {
        return request.Cookies[AccessTokenCookie] ?? string.Empty;
    }

    public static Guid GetUserIdFromAccessTokenCookie(this HttpRequest request)
    {
        var token = request.Cookies[AccessTokenCookie];
        if (string.IsNullOrEmpty(token)) return Guid.Empty;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }


    private static CookieOptions CreateCookieOptions(DateTime expiresAt) =>
        new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/api/auth"
        };
}
