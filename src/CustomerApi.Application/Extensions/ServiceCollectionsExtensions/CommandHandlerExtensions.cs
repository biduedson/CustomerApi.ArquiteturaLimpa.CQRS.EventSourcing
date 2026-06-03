using System.Reflection;
using CustomerApi.Application.Abstractions;
using CustomerApi.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Application.Extensions.ServiceCollectionsExtensions;

public static class CommandHandlerExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(IApplicationMarker));

        return services
            .AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly!)
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)));
    }
}
