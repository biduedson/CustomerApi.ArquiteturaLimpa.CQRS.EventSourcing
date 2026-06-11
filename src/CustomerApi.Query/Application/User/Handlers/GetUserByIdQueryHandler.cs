using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Query.Application.User.Queries;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;
using FluentValidation;
using MediatR;

namespace CustomerApi.Query.Application.User.Handlers;

public class GetUserByIdQueryHandler(
    IValidator<GetUserByIdQuery> validator,
    IUserReadOnlyRepository repository,
    ICacheService cacheService) : IRequestHandler<GetUserByIdQuery, Result<UserQueryModel>>
{
    public async Task<Result<UserQueryModel>> Handle(
     GetUserByIdQuery request,
     CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return Result<UserQueryModel>.Invalid(validationResult.AsErrors());

        var cacheKey = $"{nameof(GetUserByIdQuery)}_{request.Id}";

        var user = await cacheService.GetOrCreateAsync(cacheKey, () => repository.GetByIdAsync(request.Id));

        return user == null
        ? Result<UserQueryModel>.NotFound($"Nenhum usuario encontrado com o Id: {request.Id}")
        : Result<UserQueryModel>.Success(user);
    }
}
