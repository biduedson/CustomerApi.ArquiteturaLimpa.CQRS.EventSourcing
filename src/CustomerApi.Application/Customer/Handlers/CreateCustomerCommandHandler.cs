using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Application.Customer.Responses;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Customer.Handlers;

public class CreateCustomerCommandHandler(
    IValidator<CreateCustomerCommand> validator,
    ICustomerWriteOnlyRepository repository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateCustomerCommand, Result<CreatedCustomerResponse>>
{
    public async Task<Result<CreatedCustomerResponse>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result<CreatedCustomerResponse>.Invalid(validationResult.AsErrors());

        var email = Email.Create(request.Email);

        if (await repository.ExistsByEmailAsync(email))
            return Result<CreatedCustomerResponse>.Error("O endereço de e-mail informado já está em uso.");
        var customer = Domain.Entities.CustomerAggregate.Customer.Create(
            request.FirstName,
            request.LastName,
            request.Gender,
            email.Address,
            request.DateOfBirth
            );

        repository.Add(customer);

        await unitOfWork.SaveChangesAsync();

        return Result<CreatedCustomerResponse>.Created(
            new CreatedCustomerResponse(customer.Id), location: $"/api/customers/{customer.Id}");
    }
}
