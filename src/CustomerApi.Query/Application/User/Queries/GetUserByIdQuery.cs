using System;
using Ardalis.Result;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.User.Queries;

public class GetUserByIdQuery(Guid id) : IRequest<Result<UserQueryModel>>
{
    public Guid Id { get; } = id;
}
