using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Account.Commands.Emails.Change;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Account.Handlers.Emails.Change;

public class ChangeEmailCommandHandler(
    IValidator<ChangeEmailCommand> validator,
    IUserWriteOnlyRepository userRepository,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<ChangeEmailCommand, Result>
{
    public async Task<Result> Handle(
        ChangeEmailCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user is null)
            return Result.NotFound($"Nenhum usuario encontrado com o Id: {request.UserId}");

        var newEmail = Email.Create(request.Email);

        var existingEmail = await userRepository.ExistsByEmailAndIdAsync(newEmail, user.Id);

        if (existingEmail)
            return Result.Error("O endereço de e-mail informado já está em uso.");

        user.ChangeEmail(newEmail);

        userRepository.Update(user);

        await userSessionRepository.RevokeAllByUserIdAsync(user.Id, "Atualização de e-mail");

        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
