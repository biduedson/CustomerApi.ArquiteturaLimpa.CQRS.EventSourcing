using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.Application.Auth.Commands.Logout;
using CustomerApi.Application.Auth.Commands.RefreshToken;
using CustomerApi.WebApi.Extensions;
using CustomerApi.WebApi.Extensions.HttpContextExtensions;
using CustomerApi.WebApi.Extensions.ResultExtensions;
using CustomerApi.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.WebApi.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    ////////////////////////
    // POST: /api/auth/login
    ////////////////////////
    [HttpPost("login")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        command.UserAgent = Request.GetUserAgent();
        command.IpAddress = Request.GetIpAddress();

        var result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            Response.AppendAuthCookies(result.Value);
            return Ok();
        }

        return result.ToActionResult();
    }

    ////////////////////////
    // POST: /api/auth/refreshtoken
    ////////////////////////
    [AllowAnonymous]
    [HttpPost("refreshtoken")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.GetRefreshTokenCookies();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiResponse.Unauthorized());

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            UserAgent = Request.GetUserAgent(),
            IpAddress = Request.GetIpAddress()
        };

        var result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            Response.AppendAuthCookies(result.Value);
            return Ok();
        }

        return result.ToActionResult();
    }

    ////////////////////////
    // POST: /api/auth/logout
    ////////////////////////
    [HttpPost("logout")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand
        {
            RefreshToken = Request.GetRefreshTokenCookies(),
        };

        var result = await mediator.Send(command);

        if (result.IsSuccess)
        {
            Response.DeleteAuthCookies();
            return Ok();
        }

        return result.ToActionResult();
    }
}
