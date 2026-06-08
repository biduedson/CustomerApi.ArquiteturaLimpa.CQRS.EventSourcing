using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.RefreshToken;
using CustomerApi.Application.Auth.Responses;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CustomerApi.Application.Auth.Handlers.RefreshToken;

public class RefreshTokenCommandHandler(
    IValidator<RefreshTokenCommand> validator,
    IUserWriteOnlyRepository userRepository,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IOptions<JwtOptions> jwtOptions,
    IUnitOfWork unitOfWork
) : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<AuthenticationResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result<AuthenticationResponse>.Invalid(validationResult.AsErrors());

        var currentTokenHash = refreshTokenService.HashToken(request.RefreshToken);

        var userSession = await userSessionRepository.GetByRefreshTokenHashAsync(currentTokenHash);

        if (userSession is null)
            return Result<AuthenticationResponse>.Unauthorized();

        if (userSession.IsRevoked)
        {
            await userSessionRepository.RevokeAllByUserIdAsync(userSession.UserId, "Reuso de refreshtoken revogado detectado.");
            await unitOfWork.SaveChangesAsync();
            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (!userSession.IsActive)
            return Result<AuthenticationResponse>.Unauthorized();

        if (!userSession.MatchesFingerprint(request.UserAgent))
        {
            userSession.Revoke("Mudança de dispositivo detectada");
            await unitOfWork.SaveChangesAsync();
            return Result<AuthenticationResponse>.Unauthorized();

        }

        var user = await userRepository.GetByIdAsync(userSession.UserId);

        if (user is null || !user.IsActive)
            return Result<AuthenticationResponse>.Unauthorized();

        var newRefreshToken = refreshTokenService.GenerateToken();
        var newRefreshTokenHash = refreshTokenService.HashToken(newRefreshToken);
        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);

        userSession.Revoke("Revogado por rotação do refreshToken");

        var newUserSession = UserSession.Create(
            user.Id,
            newRefreshTokenHash,
            request.UserAgent,
            request.IpAddress,
            refreshTokenExpiresAt
        );

        userSessionRepository.Add(newUserSession);

        await unitOfWork.SaveChangesAsync();

        return Result<AuthenticationResponse>.Success(
            new AuthenticationResponse(accessToken, newRefreshToken, accessTokenExpiresAt, refreshTokenExpiresAt));

    }
}
