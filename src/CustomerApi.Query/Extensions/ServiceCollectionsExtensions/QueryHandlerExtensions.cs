using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CustomerApi.Query.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Query.Extensions.ServiceCollectionsExtensions;

[ExcludeFromCodeCoverage]
public static class QueryHandlerExtensions
{
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(IQueryMarker));

        return services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly))
            .AddAutoMapper(cfg => cfg.AddMaps(assembly))
            .AddValidatorsFromAssembly(assembly);
    }
}
