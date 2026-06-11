using FluentValidation;

namespace CustomerApi.Application.Account.Commands.Emails.Change;

public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("O usuario deve ser informado.");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("O e-mail deve ser informado.")
            .EmailAddress()
            .WithMessage("O e-mail informado esta com formato invalido.");
    }
}
