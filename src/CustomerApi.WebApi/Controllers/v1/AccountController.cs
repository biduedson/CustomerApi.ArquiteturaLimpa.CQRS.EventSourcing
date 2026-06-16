using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using CustomerApi.Application.Account.Commands.Emails.Change;
using CustomerApi.Application.Account.Commands.Passwords.Change;
using CustomerApi.WebApi.Extensions;
using CustomerApi.WebApi.Extensions.ResultExtensions;
using CustomerApi.WebApi.Models;
using CustomerApi.WebApi.Models.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.WebApi.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class AccountController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    ////////////////////////////////////
    // POST: /api/account/changepassword
    ////////////////////////////////////
    [Authorize]
    [HttpPost("changepassword")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Change([FromBody] ChangePasswordDto request)
    {
        var userId = Request.GetUserIdFromAccessTokenCookie();

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword,

        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            Response.DeleteAuthCookies();
            return Ok(result);
        }

        return result.ToActionResult();
    }

    /////////////////////////////////
    // POST: /api/account/changeemail
    /////////////////////////////////
    [Authorize]
    [HttpPost("changeemail")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto request)
    {
        var userId = Request.GetUserIdFromAccessTokenCookie();

        var command = new ChangeEmailCommand
        {
            UserId = userId,
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            Response.DeleteAuthCookies();
            return Ok(result);
        }

        return result.ToActionResult();
    }
}
