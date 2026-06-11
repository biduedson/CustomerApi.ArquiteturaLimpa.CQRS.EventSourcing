using FluentValidation;

namespace CustomerApi.Application.Users.Commands.Update.Profile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.Id)
           .NotEmpty();
    }
}
