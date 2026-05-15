using Ardalis.Result;
using FluentValidation;

namespace CustomerApi.Application.Customer.Commands;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(command => command.Id)
             .NotEmpty();
    }
}
