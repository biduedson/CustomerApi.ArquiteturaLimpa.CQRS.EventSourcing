using FluentValidation;

namespace CustomerApi.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();

        RuleFor(command => command.UserAgent)
        .NotEmpty()
        .MaximumLength(512);
    }
}