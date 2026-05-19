using CustomerApi.WebApi.Middlewares;

namespace CustomerApi.WebApi.Extensions;

public static class MiddlewareExtensions
{
    public static void UseErrorHandling(this IApplicationBuilder builder) =>
      builder.UseMiddleware<ErrorHandlingMiddleware>();
}
