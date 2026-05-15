using FluentValidation;

namespace CustomerApi.Application.Customer.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Email)
            .NotNull()
            .MaximumLength(254)
            .EmailAddress();
    }
}
