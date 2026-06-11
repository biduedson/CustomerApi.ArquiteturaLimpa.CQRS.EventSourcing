using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.Application.Auth.Responses;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CustomerApi.Application.Auth.Handlers.Login;

public class LoginCommandHandler(
IValidator<LoginCommand> validator,
IUserWriteOnlyRepository userRepository,
IUserSessionWriteOnlyRepository sessionRepository,
IPasswordHasher passwordHasher,
IJwtTokenGenerator jwtTokenGenerator,
IRefreshTokenService refreshTokenService,
IOptions<JwtOptions> jwtOptions,
IUnitOfWork unitOfWork
) : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return Result<AuthenticationResponse>.Invalid(validationResult.AsErrors());

        var email = Email.Create(request.Email);
        var user = await userRepository.GetByEmailAsync(email);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthenticationResponse>.Unauthorized("Credenciais inválidas.");

        var accessTokem = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = refreshTokenService.GenerateToken();
        var refreshTokenHash = refreshTokenService.HashToken(refreshToken);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);

        var existingDeviceSession = await sessionRepository.GetByUserAgentAsync(request.UserAgent);

        if (existingDeviceSession?.IsActive == true)
        {
            existingDeviceSession.Revoke(
                reason: "Usuário efetuou novo login no mesmo dispositivo",
                replacedByTokenHash: refreshTokenHash);

            sessionRepository.Update(existingDeviceSession);
        }
        else
        {
            var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            request.UserAgent,
            request.IpAddress ?? string.Empty,
            accessTokenExpiresAt);

            sessionRepository.Add(session);
        }

        await unitOfWork.SaveChangesAsync();

        return Result<AuthenticationResponse>.Success(
            new AuthenticationResponse(
                accessTokem,
                refreshToken,
                accessTokenExpiresAt,
                refreshTokenExpiresAt));
    }
}