using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
      services
          .AddScoped<WriteDbContext>()
          .AddScoped<EventStoreDbContext>()
          .AddScoped<IUnitOfWork, UnitOfWork>();

    public static IServiceCollection AddWriteOnlyRepositories(this IServiceCollection services) =>
     services
        .AddScoped<IEventStoreRepository, EventStoreRepository>()
        .AddScoped<ICustomerWriteOnlyRepository, CustomerWriteOnlyRepository>();
}

