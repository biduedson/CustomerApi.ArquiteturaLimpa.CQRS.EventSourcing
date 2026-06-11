using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Account.Commands.Passwords.Change;

public class ChangePasswordCommand() : IRequest<Result>
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(100)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
