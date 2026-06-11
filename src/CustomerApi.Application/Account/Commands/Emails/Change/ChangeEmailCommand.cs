using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Account.Commands.Emails.Change;

public class ChangeEmailCommand : IRequest<Result>
{
    [Required]
    public Guid UserId { get; set; } = default!;

    [Required]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}
