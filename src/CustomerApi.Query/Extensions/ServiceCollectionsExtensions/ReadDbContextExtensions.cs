using System.Diagnostics.CodeAnalysis;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Data.Context;
using CustomerApi.Query.Data.Mappings;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace CustomerApi.Query.Extensions.ServiceCollectionsExtensions;

[ExcludeFromCodeCoverage]
public static class ReadDbContextExtensions
{
    private static readonly object Lock = new();
    private static bool _configured;

    public static IServiceCollection AddReadDbContext(this IServiceCollection services)
    {
        services
            .AddScoped<ISynchronizeDb, NoSqlDbContext>()
            .AddScoped<IReadDbContext, NoSqlDbContext>()
            .AddScoped<NoSqlDbContext>();

        ConfigureMongo();

        return services;
    }

    private static void ConfigureMongo()
    {
        lock (Lock)
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
            new UserMap().Configure();

            _configured = true;
        }
    }
}
