using System.Diagnostics.CodeAnalysis;
using CustomerApi.Query.Data.Repositories;
using CustomerApi.Query.Data.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Query.Extensions.ServiceCollectionsExtensions;

[ExcludeFromCodeCoverage]
public static class ReadOnlyRepositoryExtensions
{
    public static IServiceCollection AddReadOnlyRepositories(this IServiceCollection services) =>
     services
        .AddScoped<ICustomerReadOnlyRepository, CustomerReadOnlyRepository>()
        .AddScoped<IUserReadOnlyRepository, UserReadOnlyRepository>();

}
