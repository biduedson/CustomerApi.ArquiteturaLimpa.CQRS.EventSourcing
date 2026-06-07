using System;

namespace CustomerApi.Application.Auth.Responses;

public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
    );

