using System;
using CustomerApi.Domain.Entities.UserAggregate;

namespace CustomerApi.WebApi.Models.Users;

public sealed record CreateUserDto(
    string Username,
    string Email,
    UserRole Role,
    string FullName,
    DateTime DateOfBirth,
    string JobTitle,
    string Password
);
