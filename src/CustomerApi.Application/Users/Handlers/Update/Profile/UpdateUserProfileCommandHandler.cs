using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Users.Commands.Update.Profile;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Users.Handlers.Update.Profile;

public class UpdateUserProfileCommandHandler(
    IValidator<UpdateUserProfileCommand> validator,
    IUserWriteOnlyRepository userRepository,
    IUserSessionWriteOnlyRepository userSessionRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateUserProfileCommand, Result>
{
    public async Task<Result> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var user = await userRepository.GetByIdAsync(request.Id);

        if (user is null)
            return Result.NotFound($"Nenhum usuario encontrado com o Id: {request.Id}");

        var newProfile = UserProfile.Create(
            request.FullName ?? user.Profile.FullName,
            request.DateOfBirth != default
            ? request.DateOfBirth
            : user.Profile.DateOfBirth,
            request.JobTitle ?? user.Profile.JobTitle
            );

        user.ChangeProfile(newProfile);

        userRepository.Update(user);

        await userSessionRepository.RevokeAllByUserIdAsync(user.Id, "Atualização de perfil");

        await unitOfWork.SaveChangesAsync();

        return Result.Success();

    }
}
