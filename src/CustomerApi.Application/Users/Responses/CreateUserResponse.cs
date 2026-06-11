using System;
using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Application.Users.Responses;

public class CreateUserResponse(Guid id) : IResponse
{
    public Guid Id { get; } = id;
}
