using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Auth.Commands.Logout;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Auth.Handlers.Logout;

public class LogoutCommandHandler(
    IValidator<LogoutCommand> validator,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(
      LogoutCommand request,
      CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var refreshTokenHash = refreshTokenService.HashToken(request.RefreshToken);
        var userSession = await userSessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash);

        if (userSession is null)
            return Result.Success();

        userSession.Revoke("Usuario efetuou logout");

        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}