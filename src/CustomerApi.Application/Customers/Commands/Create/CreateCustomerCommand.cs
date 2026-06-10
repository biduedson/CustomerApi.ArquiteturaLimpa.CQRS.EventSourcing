using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using CustomerApi.Application.Customers.Responses;
using CustomerApi.Domain.Entities.CustomerAggregate;
using MediatR;

namespace CustomerApi.Application.Customers.Commands.Create;

public class CreateCustomerCommand : IRequest<Result<CreatedCustomerResponse>>
{
    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string FirstName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Text)]
    public string LastName { get; set; } = default!;

    [Required]
    public EGender Gender { get; set; }

    [Required]
    [MaxLength(200)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}
