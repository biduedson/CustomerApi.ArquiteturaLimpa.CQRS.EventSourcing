using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Auth;
using CustomerApi.Infrastructure.Auth.Password;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure.Extensions.ServiceCollectionsExtensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<WriteDbContext>()
            .AddScoped<EventStoreDbContext>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>()
            .AddScoped<IPasswordHasher, BCryptPasswordHasher>();
}
