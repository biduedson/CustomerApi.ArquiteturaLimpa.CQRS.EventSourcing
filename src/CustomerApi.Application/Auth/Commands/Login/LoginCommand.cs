using System.Text.Json.Serialization;
using Ardalis.Result;
using CustomerApi.Application.Auth.Responses;
using MediatR;

namespace CustomerApi.Application.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthenticationResponse>>
{

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public string UserAgent { get; set; } = string.Empty;

    [JsonIgnore]
    public string? IpAddress { get; set; }
}