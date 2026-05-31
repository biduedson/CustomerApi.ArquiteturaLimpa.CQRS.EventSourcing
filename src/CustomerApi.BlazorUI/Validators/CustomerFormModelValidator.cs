using CustomerApi.BlazorUI.Models.Customers;
using FluentValidation;


namespace CustomerApi.BlazorUI.Validators;

public sealed class CustomerFormModelValidator : AbstractValidator<CustomerFormModel>
{
    public CustomerFormModelValidator()
    {
        RuleFor(customer => customer.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(customer => customer.LastName).NotEmpty().MaximumLength(100);
        RuleFor(customer => customer.Email).NotEmpty().EmailAddress().MaximumLength(180);
        RuleFor(customer => customer.Gender).NotEmpty();
        RuleFor(customer => customer.DateOfBirth).NotNull().LessThan(DateTime.Today);
    }
}