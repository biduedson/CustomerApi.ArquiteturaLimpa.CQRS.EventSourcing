using System;
using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Application.Customers.Responses;

public class CreatedCustomerResponse(Guid id) : IResponse
{
    public Guid Id { get; } = id;
}
