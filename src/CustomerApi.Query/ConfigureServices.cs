using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Data;
using CustomerApi.Query.Data.Mappings;
using CustomerApi.Query.Data.Repositories;
using CustomerApi.Query.Data.Repositories.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace CustomerApi.Query;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(IQueryMarker));

        return services
        .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly))
        .AddAutoMapper(cfg => cfg.AddMaps(assembly))
        .AddValidatorsFromAssembly(assembly);
    }

    public static IServiceCollection AddReadDbContext(this IServiceCollection services)
    {
        services
        .AddScoped<ISynchronizeDb, NoSqlDbContext>()
        .AddScoped<IReadDbContext, NoSqlDbContext>()
        .AddScoped<NoSqlDbContext>();

        ConfigureMongo();

        return services;
    }

    public static IServiceCollection AddReadOnlyRepositories(this IServiceCollection services) =>
         services.AddScoped<ICustomerReadOnlyRepository, CustomerReadOnlyRepository>();

    private static bool _configured;
    private static readonly object _lock = new();

    private static void ConfigureMongo()
    {
        lock (_lock)
        {
            if (_configured)
                return;

            BsonSerializer.TryRegisterSerializer(
                new GuidSerializer(GuidRepresentation.CSharpLegacy));

            ConventionRegistry.Register("Conventions",
                new ConventionPack
                {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
                }, _ => true);

            new CustomerMap().Configure();

            _configured = true;
        }
    }
}
