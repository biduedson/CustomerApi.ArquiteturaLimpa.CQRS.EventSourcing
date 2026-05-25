using System;
using Ardalis.Result;
using CustomerApi.Query.QueriesModel;
using MediatR;

namespace CustomerApi.Query.Application.Customer.Queries;

public class GetCustomerByIdQuery(Guid id) : IRequest<Result<CustomerQueryModel>>
{
    public Guid Id { get; } = id;
}
