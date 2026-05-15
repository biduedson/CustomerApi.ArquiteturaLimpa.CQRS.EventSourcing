using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Customer.Handlers;

public class UpdateCustomerCommandHandler(
    IValidator<UpdateCustomerCommand> validator,
    ICustomerWriteOnlyRepository repository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateCustomerCommand ,Result>
{
    public async Task<Result> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken
        )
    {
        var validationResult = await validator.ValidateAsync( request, cancellationToken );

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var customer = await repository.GetByIdAsync(request.Id);
        if (customer == null)
            return Result.NotFound($"Nenhum cliente encontrado com o Id: {request.Id}");

        var newEmail = Email.Create(request.Email);

        if (await repository.ExistsByEmailAsync(newEmail, request.Id))
            return Result.Error("O endereço de e-mail informado já está em uso.");

        customer.ChangeEmail(newEmail);

        repository.Update(customer);

        await unitOfWork.SaveChangesAsync();

        return Result.SuccessWithMessage("Atualizado com sucesso!");

    }
}
