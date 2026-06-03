using System;
using System.Threading.Tasks;
using AutoMapper;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Query.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.WebApplicationExtensions;

public static class ApplicationStartupExtensions
{
    public static async Task RunAppAsync(this WebApplication app)
    {
        await using var serviceScope = app.Services.CreateAsyncScope();

        var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();

        app.Logger.LogInformation("----- AutoMapper: validando os mapeamentos...");

        mapper.ConfigurationProvider.CompileMappings();

        app.Logger.LogInformation("----- AutoMapper: mapeamentos validados com sucesso!");

        await app.MigrateDataBasesAsync(serviceScope);

        app.Logger.LogInformation("----- Aplicação está iniciando...");

        await app.RunAsync();
    }

    private static async Task MigrateDataBasesAsync(this WebApplication app, AsyncServiceScope serviceScope)
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
