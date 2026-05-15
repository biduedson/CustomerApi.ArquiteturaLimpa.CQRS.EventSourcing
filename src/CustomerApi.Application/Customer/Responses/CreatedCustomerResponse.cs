using System;
using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Application.Customer.Responses;

public class CreatedCustomerResponse(Guid id) : IResponse
{
    public Guid Id { get; } = id;
}
