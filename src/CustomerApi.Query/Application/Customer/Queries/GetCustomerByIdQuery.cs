using System;
using CustomerApi.Query.QueriesModel;

namespace CustomerApi.Query.Application.Customer.Queries;

public class GetCustomerByIdQuery(Guid id) : IRequest<Result<CustomerQueryModel>>
{
    public Guid Id { get; } = id;
}