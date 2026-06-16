using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using CustomerApi.Application.Customers.Commands.Create;
using CustomerApi.Application.Customers.Commands.Delete;
using CustomerApi.Application.Customers.Commands.Update;
using CustomerApi.Application.Customers.Responses;
using CustomerApi.Query.Application.Customer.Queries;
using CustomerApi.Query.QueriesModel;
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
public class CustomersController(IMediator mediator) : ControllerBase
{

    #region Controller Write

    ////////////////////////
    // POST: /api/customers
    ////////////////////////
    [Authorize(Roles = "Admin,Operator")]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CreatedCustomerResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody][Required] CreateCustomerCommand command) =>
    (await mediator.Send(command)).ToActionResult();


    ///////////////////////
    // PUT: /api/customers
    //////////////////////
    [Authorize(Roles = "Admin,Operator")]
    [HttpPut]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody][Required] UpdateCustomerCommand command) =>
        (await mediator.Send(command)).ToActionResult();

    //////////////////////////////
    // DELETE: /api/customers/{id}
    //////////////////////////////
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([Required] Guid id) =>
        (await mediator.Send(new DeleteCustomerCommand(id))).ToActionResult();
    #endregion

    #region Controller Read

    ///////////////////////////
    // GET: /api/customers/{id}
    ///////////////////////////
    [Authorize(Roles = "Admin,Operator")]
    [HttpGet("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<CustomerQueryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([Required] Guid id) =>
        (await mediator.Send(new GetCustomerByIdQuery(id))).ToActionResult();

    //////////////////////
    // GET: /api/customers
    //////////////////////
    [Authorize(Roles = "Admin,Operator")]
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerQueryModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll() =>
        (await mediator.Send(new GetAllCustomerQuery())).ToActionResult();

    #endregion

}
