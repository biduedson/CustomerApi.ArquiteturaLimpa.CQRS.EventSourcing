

using System.Globalization;
using System.IO.Compression;
using Asp.Versioning;
using CorrelationId;
using CorrelationId.DependencyInjection;
using CustomerApi.Application;
using CustomerApi.Core;
using CustomerApi.Core.Extensions;
using CustomerApi.Infrastructure;
using CustomerApi.WebApi.Extensions;
using FluentValidation;
using FluentValidation.Resources;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using StackExchange.Profiling;

// Cria o builder da aplicação.
var builder = WebApplication.CreateBuilder(args);

// Configura serviços base da API.
builder.Services
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

// Configura documentação e controllers.
builder.Services.AddOpenApi();
builder.Services.AddDataProtection();
builder.Services.AddControllers()
      .ConfigureApiBehaviorOptions(behaviorOptions =>
      {
             behaviorOptions.SuppressMapClientErrors = true;
             behaviorOptions.SuppressModelStateInvalidFilter = true;
      })
      .AddJsonOptions(_ => { });

// Registra módulos da aplicação.
builder.Services
       .ConfigureAppSettings()
       .AddInfrastructure()
       .AddCommandHandlers()
       .AddWriteDbContext(builder.Environment)
       .AddCAcheService(builder.Configuration)
       .AddHealthChecks(builder.Configuration)
       .AddDefaultCorrelationId();

// Configura profiler da aplicação.
builder.Services.AddMiniProfiler(options =>
{
       options.RouteBasePath = "/profiler";
       options.ColorScheme = ColorScheme.Dark;
       options.EnableServerTimingHeader = true;
       options.TrackConnectionOpenClose = true;
       options.EnableDebugMode = builder.Environment.IsDevelopment();
}).AddEntityFramework();

// Valida dependências no startup.
builder.Host.UseDefaultServiceProvider((context, serviceProviderOptions) =>
{
       serviceProviderOptions.ValidateScopes = context.HostingEnvironment.IsDevelopment();
       serviceProviderOptions.ValidateOnBuild = true;
});

// Define opções globais de validação.
ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;
ValidatorOptions.Global.LanguageManager = new LanguageManager { Enabled = true, Culture = new CultureInfo("en-US") };

// Constrói a aplicação.
var app = builder.Build();


// Exibe detalhes de erro em desenvolvimento.
if (app.Environment.IsDevelopment())
{
       app.UseDeveloperExceptionPage();
}

// Expõe o endpoint de health check.
app.UseHealthChecks("/health", new HealthCheckOptions
{
       Predicate = _ => true,
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Mapeia a especificação OpenAPI.
app.MapOpenApi();

// Configura a UI da documentação.
app.MapScalarApiReference(scalarOptions =>
{
       scalarOptions.DarkMode = true;
       scalarOptions.DotNetFlag = false;
       scalarOptions.DocumentDownloadType = DocumentDownloadType.None; ;
       scalarOptions.HideModels = true;
       scalarOptions.Title = "Shop API";
});

// Configura o pipeline HTTP.
app.UseErrorHandling();
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseMiniProfiler();
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Inicia a aplicação.
await app.RunAppAsync();
