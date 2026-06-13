using System;
using System.Threading.Tasks;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Query.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.WebApplicationExtensions;

public static class DatabaseMigrationExtensions
{
    internal static async Task MigrateDatabasesAsync(this WebApplication app, AsyncServiceScope serviceScope)
    {
        await using var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
        await using var eventStoreDbContext = serviceScope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        using var readDbContext = serviceScope.ServiceProvider.GetRequiredService<IReadDbContext>();

        try
        {
            await app.MigrateDbContextAsync(writeDbContext);
            await app.MigrateDbContextAsync(eventStoreDbContext);
            await app.MigrateMongoDbContextAsync(readDbContext);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Ocorreu uma exceção ao inicializar a aplicação: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task MigrateDbContextAsync<TDbContext>(this WebApplication app, TDbContext dbContext)
        where TDbContext : DbContext
    {
        var dbName = dbContext.Database.GetDbConnection().Database;

        app.Logger.LogInformation("----- {DbName}: verificando banco de dados...", dbName);
        var exists = await dbContext.Database.CanConnectAsync();

        if (!exists)
        {
            // banco não existe — cria e aplica todas as migrations
            app.Logger.LogInformation("----- {DbName}: banco não encontrado — criando...", dbName);

            await dbContext.Database.MigrateAsync();

            app.Logger.LogInformation("----- {DbName}: banco criado com sucesso!", dbName);

            return;
        }

        app.Logger.LogInformation("----- {DbName}: verificando migrações pendentes...", dbName);

        if (dbContext.Database.HasPendingModelChanges())
        {
            app.Logger.LogInformation("----- {DbName}: criando e migrando o banco de dados...", dbName);

            await dbContext.Database.MigrateAsync();

            app.Logger.LogInformation("----- {DbName}: banco de dados migrado com sucesso!", dbName);
        }
        else
        {
            app.Logger.LogInformation("----- {DbName}: todas as migrações estão atualizadas.", dbName);
        }
    }

    private static async Task MigrateMongoDbContextAsync(this WebApplication app, IReadDbContext readDbContext)
    {
        app.Logger.LogInformation("----- MongoDB: criando coleções...");

        await readDbContext.CreateCollectionsAsync();

        app.Logger.LogInformation("----- MongoDB: coleções criadas com sucesso!");
    }
}
