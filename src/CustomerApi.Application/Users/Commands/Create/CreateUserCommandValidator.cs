
using FluentValidation;

namespace CustomerApi.Application.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.AuthenticatedUserId)
        .NotEmpty();

        RuleFor(command => command.Username)
        .NotEmpty();

        RuleFor(command => command.Email)
         .NotEmpty()
         .EmailAddress();

        RuleFor(command => command.Role)
        .NotEmpty();

        RuleFor(command => command.FullName)
         .NotEmpty();

        RuleFor(command => command.JobTitle)
         .NotEmpty();

        RuleFor(command => command.Password)
        .NotEmpty();
    }
}
