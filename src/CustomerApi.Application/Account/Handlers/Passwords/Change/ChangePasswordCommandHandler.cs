using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Account.Commands.Passwords.Change;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Account.Handlers.Passwords.Change;

public class ChangePasswordCommandHandler(
    IValidator<ChangePasswordCommand> validator,
    IUserWriteOnlyRepository userRepository,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user is null)
            return Result.Unauthorized();

        var currentPasswordIsValid = passwordHasher.Verify(
            request.CurrentPassword,
            user.PasswordHash);

        if (!currentPasswordIsValid)
            return Result.Unauthorized();

        var newPassword = Password.Create(request.NewPassword);

        var newPasswordHash = passwordHasher.Hash(newPassword.Value);

        user.ChangePassword(newPasswordHash);

        userRepository.Update(user);

        await userSessionRepository.RevokeAllByUserIdAsync(
            user.Id,
            "Troca de senha");

        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
