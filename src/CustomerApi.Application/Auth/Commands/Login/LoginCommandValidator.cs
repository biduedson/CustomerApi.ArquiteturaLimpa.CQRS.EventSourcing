using FluentValidation;

namespace CustomerApi.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.UserAgent)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(command => command.IpAddress)
            .MaximumLength(64);
    }
}