using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using CustomerApi.Domain.Entities.UserAggregate;
using MediatR;

namespace CustomerApi.Application.Users.Commands.Update.Role;

public class UpdateUserRoleCommand : IRequest<Result>
{
    [Required]
    public Guid Id { get; set; } = default!;

    [Required]
    public UserRole Role { get; set; }
}
