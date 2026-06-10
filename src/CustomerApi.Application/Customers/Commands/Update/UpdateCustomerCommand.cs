using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Customers.Commands.Update;

public class UpdateCustomerCommand() : IRequest<Result>
{
    [Required]
    public Guid Id { get; set; } = default!;

    [Required]
    [MaxLength(254)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;
}
