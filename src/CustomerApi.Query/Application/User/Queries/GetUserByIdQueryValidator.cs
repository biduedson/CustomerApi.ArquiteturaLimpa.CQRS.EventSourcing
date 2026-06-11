using FluentValidation;

namespace CustomerApi.Query.Application.User.Queries;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("O ID do Usuario não pode estar vazio.");
    }
}
