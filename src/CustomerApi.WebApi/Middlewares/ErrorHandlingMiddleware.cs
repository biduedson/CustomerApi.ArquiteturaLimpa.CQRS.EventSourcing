using System;
using System.Net.Mime;
using System.Threading.Tasks;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Exceptions;
using CustomerApi.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Middlewares;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IHostEnvironment enviroment)
{
    private const string ErrorMessage = "Ocorreu um erro interno ao processar sua solicitação.";

    private static readonly string ApiResponseJson = ApiResponse.InternalServerError(ErrorMessage).ToJson()!;

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DomainException domainEx:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsJsonAsync(
                        ApiResponse.BadRequest(domainEx.Message));
                    break;

                default:
                    logger.LogError(ex, "Uma exceção inesperada foi lançada: {Message}", ex.Message);
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (enviroment.IsDevelopment())
                    {
                        httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
                        await httpContext.Response.WriteAsync(ex.ToString());
                    }
                    else
                    {
                        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                        await httpContext.Response.WriteAsync(ApiResponseJson);
                    }
                    break;
            }
        }
    }

}
