using FluentValidation;
namespace CustomerApi.Application.Customers.Commands.Delete;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(command => command.Id)
             .NotEmpty();
    }
}
