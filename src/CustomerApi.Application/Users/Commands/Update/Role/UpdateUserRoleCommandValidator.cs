using FluentValidation;

namespace CustomerApi.Application.Users.Commands.Update.Role;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Role)
            .IsInEnum();
    }
}
