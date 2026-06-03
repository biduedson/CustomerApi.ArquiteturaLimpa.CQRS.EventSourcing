using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Auth;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Infrastructure.Data.Repositories;
using CustomerApi.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure;

public static class ConfigureServices
{

    public static void AddMemoryCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, MemoryCacheService>();

    public static void AddDistributedCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, DistributedCacheService>();
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
      services
          .AddScoped<WriteDbContext>()
          .AddScoped<EventStoreDbContext>()
          .AddScoped<IUnitOfWork, UnitOfWork>()
          .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

    public static IServiceCollection AddWriteOnlyRepositories(this IServiceCollection services) =>
     services
        .AddScoped<IEventStoreRepository, EventStoreRepository>()
        .AddScoped<ICustomerWriteOnlyRepository, CustomerWriteOnlyRepository>()
        .AddScoped<IUserWriteOnlyRepository, UserWriteOnlyRepository>();
}

