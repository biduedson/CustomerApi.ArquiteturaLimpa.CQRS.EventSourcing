using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Query.Application.User.Queries;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.User.Handlers;

public class GetAllUserQueryHandler(IUserReadOnlyRepository repository, ICacheService cacheService)
: IRequestHandler<GetAllUserQuery, Result<IEnumerable<UserQueryModel>>>
{
    private const string CacheKey = nameof(GetAllUserQuery);

    public async Task<Result<IEnumerable<UserQueryModel>>> Handle(
    GetAllUserQuery request,
    CancellationToken cancellationToken
   )
    {
        return Result<IEnumerable<UserQueryModel>>.Success(
          await cacheService.GetOrCreateAsync(CacheKey, repository.GetAllAsync));

    }
}
