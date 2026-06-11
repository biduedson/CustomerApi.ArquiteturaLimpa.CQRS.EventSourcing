using System;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Users.Commands.Delete;

public class DeleteUserCommand(Guid id) : IRequest<Result>
{
    public Guid Id { get; } = id;
}
