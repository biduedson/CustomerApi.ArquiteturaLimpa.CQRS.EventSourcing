
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using CustomerApi.Application.Users.Commands.Create;
using CustomerApi.Application.Users.Commands.Delete;
using CustomerApi.Application.Users.Commands.Update.Profile;
using CustomerApi.Application.Users.Commands.Update.Role;
using CustomerApi.Application.Users.Responses;
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
public class UsersController(IMediator mediator) : ControllerBase
{
    ////////////////////////
    // POST: /api/users
    ////////////////////////
    [Authorize(Roles = "Admin,Operator")]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> Create([FromBody] CreateUserCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    //////////////////////////////
    // DELETE: /api/users/{id}
    //////////////////////////////
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([Required] Guid id) =>
        (await mediator.Send(new DeleteUserCommand(id))).ToActionResult();

    //////////////////////////////
    // PUT: /api/users/profile
    //////////////////////////////
    [Authorize(Roles = "Admin")]
    [HttpPut("profile")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody][Required] UpdateUserProfileCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    ///////////////////////////
    // PUT: /api/users/role
    ///////////////////////////
    [Authorize(Roles = "Admin")]
    [HttpPut("role")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRole([FromBody][Required] UpdateUserRoleCommand command) =>
        (await mediator.Send(command)).ToActionResult();
}
