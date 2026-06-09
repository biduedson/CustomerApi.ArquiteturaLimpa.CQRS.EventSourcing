using FluentValidation;

namespace CustomerApi.Application.Account.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
           .NotEmpty()
           .WithMessage("O usuário deve ser informado.");

        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .WithMessage("A senha atual deve ser informada.");

        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .WithMessage("A nova senha deve ser informada.");

        RuleFor(command => command.ConfirmPassword)
            .NotEmpty()
            .WithMessage("A confirmação da nova senha deve ser informada.");

        RuleFor(command => command)
            .Must(command => command.NewPassword == command.ConfirmPassword)
            .WithMessage("A confirmação da nova senha não corresponde à nova senha informada.");
    }
}
