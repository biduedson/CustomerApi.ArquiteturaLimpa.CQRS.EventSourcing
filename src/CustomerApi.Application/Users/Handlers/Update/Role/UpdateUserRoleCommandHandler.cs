using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Users.Commands.Update.Role;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Users.Handlers.Update.Role;

public class UpdateUserRoleCommandHandler(
    IValidator<UpdateUserRoleCommand> validator,
    IUserWriteOnlyRepository userRepository,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateUserRoleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateUserRoleCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var user = await userRepository.GetByIdAsync(request.Id);

        if (user is null)
            return Result.NotFound($"Nenhum usuario encontrado com o Id: {request.Id}");

        user.ChangeRole(request.Role);

        userRepository.Update(user);

        await userSessionRepository.RevokeAllByUserIdAsync(user.Id, "Atualização de perfil de acesso");

        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
