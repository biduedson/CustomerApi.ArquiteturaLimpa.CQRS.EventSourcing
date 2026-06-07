using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.WebApi.Extensions;
using CustomerApi.WebApi.Extensions.HttpContextExtensions;
using CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;
using CustomerApi.WebApi.Models;
using MediatR;
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


}