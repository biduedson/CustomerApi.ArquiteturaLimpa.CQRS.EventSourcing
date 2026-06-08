using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CustomerApi.Application.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Result>
{
    [Required]
    [MaxLength(128)]
    [DataType(DataType.Text)]
    public string RefreshToken { get; set; } = string.Empty;
}