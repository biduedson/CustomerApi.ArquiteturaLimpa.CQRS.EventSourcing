using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.WebApplicationExtensions;

public static class MappingValidationExtensions
{
    internal static void ValidateMappings(this WebApplication app, AsyncServiceScope serviceScope)
    {
        var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();

        app.Logger.LogInformation("----- AutoMapper: validando os mapeamentos...");

        mapper.ConfigurationProvider.CompileMappings();

        app.Logger.LogInformation("----- AutoMapper: mapeamentos validados com sucesso!");
    }
}
