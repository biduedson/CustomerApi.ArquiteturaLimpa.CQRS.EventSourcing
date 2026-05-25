using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Query.Application.Customer.Queries;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;

namespace CustomerApi.Query.Application.Customer.Handlers;

public class GetAllCustomerQueryHandler(ICustomerReadOnlyRepository repository, ICacheService cacheService)
   : IRequestHandler<GetAllCustomerQuery, Result<IEnumerable<CustomerQueryModel>>>
{
    private const string CacheKey = nameof(GetAllCustomerQuery);

    public async Task<Result<IEnumerable<CustomerQueryModel>>> Handle(
     GetAllCustomerQuery request,
     CancellationToken cancellationToken
    )
    {
        return Result<IEnumerable<CustomerQueryModel>>.Success(
          await cacheService.GetOrCreateAsync(CacheKey, repository.GetAllAsync));

    }
}
