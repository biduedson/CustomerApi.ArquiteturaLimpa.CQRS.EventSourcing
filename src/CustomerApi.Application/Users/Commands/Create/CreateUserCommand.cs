using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using CustomerApi.Application.Users.Responses;
using CustomerApi.Domain.Entities.UserAggregate;
using MediatR;

namespace CustomerApi.Application.Users.Commands.Create;

public class CreateUserCommand : IRequest<Result<CreateUserResponse>>
{
    [Required]
    [DataType(DataType.Text)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Text)]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
