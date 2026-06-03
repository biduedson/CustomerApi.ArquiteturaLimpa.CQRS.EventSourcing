

using System.Globalization;
using CorrelationId;
using CorrelationId.DependencyInjection;
using CustomerApi.Application.Extensions.ServiceCollectionsExtensions;
using CustomerApi.Core.Extensions.ServiceCollectionsExtensions;
using CustomerApi.Infrastructure.Extensions.ServiceCollectionsExtensions;
using CustomerApi.Query.Extensions.ServiceCollectionsExtensions;
using CustomerApi.WebApi.Extensions.ApplicationBuilderExtensions;
using CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;
using CustomerApi.WebApi.Extensions.WebApplicationExtensions;
using FluentValidation;
using FluentValidation.Resources;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

// Cria o builder da aplicação.
var builder = WebApplication.CreateBuilder(args);

// Configura serviços base da API.
builder.Services
       .AddApiConfiguration();

// Registra módulos da aplicação.
builder.Services
       .ConfigureAppSettings()
       .AddInfrastructure()
       .AddCommandHandlers()
       .AddQueryHandlers()
       .AddWriteDbContext(builder.Environment)
       .AddWriteOnlyRepositories()
       .AddReadDbContext()
       .AddReadOnlyRepositories()
       .AddCacheService(builder.Configuration)
       .AddHealthChecks(builder.Configuration)
       .AddDefaultCorrelationId()
       .AddSecurityServices(builder.Configuration)
       .AddObservability(builder.Environment);

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
    scalarOptions.Title = "Customer API";
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
