using System.Collections.Generic;
using Ardalis.Result;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.Customer.Queries;

public class GetAllCustomerQuery : IRequest<Result<IEnumerable<CustomerQueryModel>>>;
