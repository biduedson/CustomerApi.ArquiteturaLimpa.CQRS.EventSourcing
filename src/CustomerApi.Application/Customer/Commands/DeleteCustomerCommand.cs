using System;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Customer.Commands;

public class DeleteCustomerCommand(Guid id) : IRequest<Result>
{
    public Guid Id { get; } = id;
}
