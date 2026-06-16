using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.WebApplicationExtensions;

public static class ApplicationStartupExtensions
{
    public static async Task RunAppAsync(this WebApplication app)
    {
        await using var serviceScope = app.Services.CreateAsyncScope();

        app.ValidateMappings(serviceScope);

        await app.MigrateDatabasesAsync(serviceScope);

        await app.SeedAdminUserAsync();

        app.Logger.LogInformation("----- Aplicação está iniciando...");

        await app.RunAsync();
    }
}
