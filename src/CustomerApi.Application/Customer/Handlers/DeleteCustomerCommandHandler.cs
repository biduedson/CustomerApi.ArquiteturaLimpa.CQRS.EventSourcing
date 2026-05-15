using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Application.Customer.Commands;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using FluentValidation;
using MediatR;

namespace CustomerApi.Application.Customer.Handlers;

public class DeleteCustomerCommandHandler(
    IValidator<DeleteCustomerCommand> validator,
    ICustomerWriteOnlyRepository repository,
    IUnitOfWork unitOfWork
    )  : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCustomerCommand request,
        CancellationToken cancellationToken
        )
    {
        var  validationResult = await validator.ValidateAsync( request, cancellationToken );

        if (!validationResult.IsValid)
            return Result.Invalid(validationResult.AsErrors());

        var customer = await repository.GetByIdAsync(request.Id);

        if (customer == null)
            return Result.NotFound($"Nenhum cliente encontrado com o Id: {request.Id}");

        repository.Remove(customer);

        await unitOfWork.SaveChangesAsync();

        return Result.SuccessWithMessage("Customer removido com sucesso!");
    }
}
