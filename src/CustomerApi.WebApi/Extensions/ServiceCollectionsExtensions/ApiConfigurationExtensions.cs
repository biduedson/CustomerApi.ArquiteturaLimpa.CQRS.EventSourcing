using System.IO.Compression;
using Asp.Versioning;
using CustomerApi.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class ApiConfigurationExtensions
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services
            .Configure<GzipCompressionProviderOptions>(compressionOptions => compressionOptions.Level = CompressionLevel.Fastest)
            .Configure<JsonOptions>(jsonOptions => jsonOptions.JsonSerializerOptions.Configure())
            .Configure<RouteOptions>(routeOptions => routeOptions.LowercaseUrls = true)
            .AddHttpClient()
            .AddHttpContextAccessor()
            .AddResponseCompression(compressionOptions =>
            {
                compressionOptions.EnableForHttps = true;
                compressionOptions.Providers.Add<GzipCompressionProvider>();
            })
            .AddEndpointsApiExplorer()
            .AddApiVersioning(versioningOptions =>
            {
                versioningOptions.DefaultApiVersion = ApiVersion.Default;
                versioningOptions.ReportApiVersions = true;
                versioningOptions.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(explorerOptions =>
            {
                explorerOptions.GroupNameFormat = "'v'VVV";
                explorerOptions.SubstituteApiVersionInUrl = true;
            });
        services.AddOpenApi();
        services.AddDataProtection();
        services.AddControllers()
                .ConfigureApiBehaviorOptions(behaviorOptions =>
                {
                    behaviorOptions.SuppressMapClientErrors = true;
                    behaviorOptions.SuppressModelStateInvalidFilter = true;
                })
                 .AddJsonOptions(_ => { });

        return services;
    }

}
