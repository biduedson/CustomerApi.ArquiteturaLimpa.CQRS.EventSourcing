using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure.Extensions.ServiceCollectionsExtensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddWriteOnlyRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IEventStoreRepository, EventStoreRepository>()
            .AddScoped<ICustomerWriteOnlyRepository, CustomerWriteOnlyRepository>()
            .AddScoped<IUserWriteOnlyRepository, UserWriteOnlyRepository>()
            .AddScoped<IUserSessionWriteOnlyRepository, UserSessionWriteOnlyRepository>();
}
