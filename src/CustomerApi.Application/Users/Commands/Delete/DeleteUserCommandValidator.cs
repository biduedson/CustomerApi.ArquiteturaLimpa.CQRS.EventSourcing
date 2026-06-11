using FluentValidation;

namespace CustomerApi.Application.Users.Commands.Delete;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.Id)
        .NotEmpty();
    }
}
