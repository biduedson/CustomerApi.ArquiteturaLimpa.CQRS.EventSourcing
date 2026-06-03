using CustomerApi.WebApi.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CustomerApi.WebApi.Extensions.ApplicationBuilderExtensions;

public static class MiddlewareExtensions
{
    public static void UseErrorHandling(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ErrorHandlingMiddleware>();
}
