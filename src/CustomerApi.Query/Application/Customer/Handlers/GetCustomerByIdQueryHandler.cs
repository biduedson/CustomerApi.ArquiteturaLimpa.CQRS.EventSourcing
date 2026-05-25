using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result.FluentValidation;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Query.Application.Customer.Queries;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.Customer.Handlers;

public class GetCustomerByIdQueryHandler(
    IValidator<GetCustomerByIdQuery> validator,
    ICustomerReadOnlyRepository repository,
    ICacheService cacheService) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerQueryModel>>
{
    public async Task<Result<CustomerQueryModel>> Handle(
     GetCustomerByIdQuery request,
     CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid) return Result<CustomerQueryModel>.Invalid(validationResult.AsErrors());

        var cacheKey = $"{nameof(GetCustomerByIdQuery)}_{request.Id}";

        var customer = await cacheService.GetOrCreateAsync(cacheKey, () => repository.GetByIdAsync(request.Id));

        return customer == null
        ? Result<CustomerQueryModel>.NotFound($"Nenhum cliente encontrado com o Id: {request.Id}")
        : Result<CustomerQueryModel>.Success(customer);
    }
}