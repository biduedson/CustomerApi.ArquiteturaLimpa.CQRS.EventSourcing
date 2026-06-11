using System.Collections.Generic;
using Ardalis.Result;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.User.Queries;

public class GetAllUserQuery : IRequest<Result<IEnumerable<UserQueryModel>>>;
