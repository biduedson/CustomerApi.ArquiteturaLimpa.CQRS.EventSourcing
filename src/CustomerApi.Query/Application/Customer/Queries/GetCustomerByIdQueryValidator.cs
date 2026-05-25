using FluentValidation;

namespace CustomerApi.Query.Application.Customer.Queries;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("O ID do cliente não pode estar vazio.");
    }
}
