using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Users.Commands.Delete;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Users.Handlers.Delete;

public class DeleteUserCommandHandler(
    IValidator<DeleteUserCommand> validator,
    IUserWriteOnlyRepository repository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var user = await repository.GetByIdAsync(request.Id);

        if (user is null)
            return Result.NotFound($"Nenhum usuario encontrado com o Id: {request.Id}");

        user.Delete();

        repository.Remove(user);

        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
