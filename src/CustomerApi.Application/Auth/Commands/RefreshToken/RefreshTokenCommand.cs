using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using CustomerApi.Application.Auth.Responses;
using MediatR;

namespace CustomerApi.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthenticationResponse>>
{
    [Required]
    [MaxLength(128)]
    [DataType(DataType.Text)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    [DataType(DataType.Text)]
    public string UserAgent { get; set; } = string.Empty;

    [MaxLength(64)]
    [DataType(DataType.Text)]
    public string? IpAddress { get; set; }
}