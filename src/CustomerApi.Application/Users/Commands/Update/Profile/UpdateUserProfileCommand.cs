using System;
using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Users.Commands.Update.Profile;

public class UpdateUserProfileCommand : IRequest<Result>
{
    [Required]
    public Guid Id { get; set; } = default!;


    [DataType(DataType.Text)]
    [MaxLength(200)]
    public string? FullName { get; set; } = default!;

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = default!;

    [DataType(DataType.Text)]
    [MaxLength(100)]
    public string JobTitle { get; set; } = default!;
}
