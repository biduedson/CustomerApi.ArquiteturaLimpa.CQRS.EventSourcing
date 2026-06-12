using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Application.Users.Commands.Create;
using CustomerApi.Application.Users.Responses;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;


namespace CustomerApi.Application.Users.Handlers.Create;

public class CreateUserCommandHandler(
    IValidator<CreateUserCommand> validator,
    IUserWriteOnlyRepository repository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async Task<Result<CreateUserResponse>> Handle(
     CreateUserCommand request,
     CancellationToken cancellationToken
    )
    {
        var validationResul = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResul.IsValid)
            return Result<CreateUserResponse>.Invalid(validationResul.AsErrors());

        var authenticatedUser = await repository.GetByIdAsync(request.AuthenticatedUserId);

        if (authenticatedUser is null)
            return Result<CreateUserResponse>.Unauthorized();

        if (request.Role == UserRole.Admin && authenticatedUser.Role != UserRole.Admin)
            return Result<CreateUserResponse>.Forbidden();

        var existingUserName = await repository.ExistsByUserNameAsync(request.Username);

        if (existingUserName)
            return Result<CreateUserResponse>.Error("O Username informado esta indisponível.");

        var email = Email.Create(request.Email);

        var existingEmail = await repository.ExistsByEmailAsync(email);

        if (existingEmail)
            return Result<CreateUserResponse>.Error("O endereço de e-mail informado já está em uso.");

        var password = Password.Create(request.Password);

        var passworHash = passwordHasher.Hash(password.ToString()!);

        var user = User.Create(
            request.Username,
            email.Address,
            request.Role,
            request.FullName,
            request.DateOfBirth,
            request.JobTitle,
            passworHash);

        repository.Add(user);

        await unitOfWork.SaveChangesAsync();
        return Result<CreateUserResponse>.Created(
             new CreateUserResponse(user.Id), location: $"/api/users/{user.Id}");
    }
}
