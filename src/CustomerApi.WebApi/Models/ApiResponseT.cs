using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace CustomerApi.WebApi.Models;

public class ApiResponse<TResult> : ApiResponse
{
    [JsonConstructor]
    public ApiResponse(
        TResult? result,
        bool success,
        string successMessage,
        int statusCode,
        string? errorCode,
        IEnumerable<ApiErrorResponse> errors
        ) : base(success, successMessage, statusCode, errorCode, errors)
    {
        Result = result!;
    }

    public ApiResponse()
    {
    }
    public TResult? Result { get; private init; }

    public static ApiResponse<TResult> Ok(TResult result) =>
    new() { Success = true, StatusCode = StatusCodes.Status200OK, Result = result };
    public static ApiResponse<TResult> Ok(TResult result, string successMessage) =>
   new() { Success = true, StatusCode = StatusCodes.Status200OK, Result = result, SuccessMessage = successMessage };
    public static ApiResponse<TResult> Created(TResult result) =>
    new() { Success = true, StatusCode = StatusCodes.Status201Created, Result = result };
}
