using System;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace CustomerApi.IntegrationTests.Controllers;

public abstract class ControllerTestsBase : IAsyncLifetime
{
    // Configurações usadas para simular o ambiente real da API durante os testes.
    // A mesma chave/issuer/audience precisa bater com a configuração de autenticação da WebApi.
    private const string ConnectionString = "Data Source=:memory:";
    private const string JwtIssuer = "CustomerApi";
    private const string JwtAudience = "CustomerApi.BlazorUI";
    private const string JwtSecret = "CHANGE_THIS_SECRET_TO_A_LONG_SECURE_KEY_WITH_AT_LEAST_32_CHARACTERS";

    // O SQLite em memória só mantém os dados enquanto a conexão estiver aberta.
    // Por isso as conexões ficam na base do teste e são abertas antes de cada cenário.
    private readonly SqliteConnection _eventStoreDbContextSqlite = new(ConnectionString);
    private readonly SqliteConnection _writeDbContextSqlite = new(ConnectionString);

    // Abre os bancos em memória antes do teste usar o WebApplicationFactory.
    public async Task InitializeAsync()
    {
        await _writeDbContextSqlite.OpenAsync();
        await _eventStoreDbContextSqlite.OpenAsync();
    }

    // Fecha as conexões ao final para liberar os recursos do SQLite em memória.
    public async Task DisposeAsync()
    {
        await _writeDbContextSqlite.DisposeAsync();
        await _eventStoreDbContextSqlite.DisposeAsync();
    }

    // Cria a aplicação de teste já com as substituições comuns dos controllers.
    // configureServices permite trocar serviços por mocks/fakes antes da API subir.
    // configureServiceScope permite semear dados no banco depois do container estar montado.
    protected TestApplicationFactory InitializeWebAppFactory(
        Action<IServiceCollection> configureServices = null,
        Action<IServiceScope> configureServiceScope = null)
    {
        return new TestApplicationFactory(
            _writeDbContextSqlite,
            _eventStoreDbContextSqlite,
            configureServices,
            configureServiceScope);
    }

    // Evita redirect automático para o teste validar o status HTTP retornado pela própria action.
    protected static WebApplicationFactoryClientOptions CreateClientOptions() => new() { AllowAutoRedirect = false };

    // Atalho para rotas que exigem perfil Admin, como Delete ou alterações administrativas.
    protected static void AuthenticateAsAdmin(HttpClient httpClient)
    {
        AuthenticateAs(httpClient, UserRole.Admin);
    }

    // Versão usada quando o teste precisa que o token represente um usuário específico.
    // Exemplo: CreateUser usa o Id do usuário autenticado para validar permissões.
    protected static void AuthenticateAsAdmin(HttpClient httpClient, Guid userId)
    {
        AuthenticateAs(httpClient, UserRole.Admin, userId);
    }

    // Adiciona o access token no cookie esperado pela WebApi.
    // Assim os testes passam pelo pipeline real de autorização do ASP.NET.
    protected static void AuthenticateAs(HttpClient httpClient, UserRole role, Guid? userId = null)
    {
        var token = CreateAccessToken(role, userId);
        httpClient.DefaultRequestHeaders.Add("Cookie", $"access_Token={token}");
    }

    // Adiciona o refresh token no cookie que AuthController e Logout esperam ler.
    protected static void AddRefreshTokenCookie(HttpClient httpClient, string refreshToken = "refresh-token")
    {
        httpClient.DefaultRequestHeaders.Add("Cookie", $"refresh_Token={refreshToken}");
    }

    // Gera um JWT válido para o middleware de autenticação da aplicação.
    // O claim sub é importante porque alguns controllers extraem dele o Id do usuário.
    private static string CreateAccessToken(UserRole role, Guid? userId = null)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var authenticatedUserId = userId ?? Guid.NewGuid();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, authenticatedUserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()),
            new Claim(ClaimTypes.Name, "admin.test"),
            new Claim(ClaimTypes.Email, "admin.test@test.com"),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Wrapper da WebApplicationFactory usado para esconder o tipo Program da WebApi.
    // O Program é internal no projeto web; se ele aparecer em métodos protected/public,
    // o compilador acusa acessibilidade inconsistente nos testes. Este wrapper mantém
    // a factory acessível para as classes de teste sem vazar esse detalhe.
    protected sealed class TestApplicationFactory : IDisposable, IAsyncDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;

        // Monta uma instância real da WebApi em memória, mas troca dependências externas
        // por alternativas controladas para testes de integração.
        public TestApplicationFactory(
            SqliteConnection writeDbContextSqlite,
            SqliteConnection eventStoreDbContextSqlite,
            Action<IServiceCollection> configureServices,
            Action<IServiceScope> configureServiceScope)
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(hostBuilder =>
                {
                    // Força configurações simples para impedir que o teste tente usar
                    // bancos, cache ou serviços externos da aplicação real.
                    hostBuilder.UseSetting("ConnectionStrings:SqlConnection", "InMemory");
                    hostBuilder.UseSetting("ConnectionStrings:NoSqlConnection", "InMemory");
                    hostBuilder.UseSetting("ConnectionStrings:CacheConnection", "InMemory");

                    hostBuilder.UseSetting("CacheOptions:AbsoluteExpirationInHours", "1");
                    hostBuilder.UseSetting("CacheOptions:SlidingExpirationInSeconds", "30");

                    // O ambiente Testing permite que a API carregue configurações próprias de teste.
                    hostBuilder.UseEnvironment("Testing");

                    // Remove logs do console durante a suíte para deixar a saída dos testes limpa.
                    hostBuilder.ConfigureLogging(logging => logging.ClearProviders());

                    hostBuilder.ConfigureServices(services =>
                    {
                        // Remove registros reais para evitar conflito com os DbContexts de teste.
                        services.RemoveAll<DbConnection>();
                        services.RemoveAll<DbContextOptions>();
                        services.RemoveAll<WriteDbContext>();
                        services.RemoveAll<DbContextOptions<WriteDbContext>>();
                        services.RemoveAll<EventStoreDbContext>();
                        services.RemoveAll<DbContextOptions<EventStoreDbContext>>();
                        services.RemoveAll<NoSqlDbContext>();
                        services.RemoveAll<ISynchronizeDb>();

                        // Usa SQLite em memória para testar EF Core com comportamento relacional.
                        // Isso é mais próximo do banco real do que o provider InMemory do EF.
                        services.AddDbContext<WriteDbContext>(
                            options => options
                                .UseSqlite(writeDbContextSqlite)
                                .ConfigureWarnings(warningBuilder => warningBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                        // Mantém o Event Store separado, como na aplicação, também em SQLite memória.
                        services.AddDbContext<EventStoreDbContext>(
                            options => options
                                .UseSqlite(eventStoreDbContextSqlite)
                                .ConfigureWarnings(warningBuilder => warningBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                        // O read model NoSQL e a sincronização não são o foco dos testes de controller.
                        // Por isso são substituídos por mocks, e cada teste troca repositórios quando precisa.
                        services.AddSingleton(_ => Substitute.For<IReadDbContext>());
                        services.AddSingleton(_ => Substitute.For<ISynchronizeDb>());

                        // Gancho para o teste substituir serviços específicos, como IMediator ou repositórios.
                        configureServices?.Invoke(services);

                        // Cria um escopo temporário para garantir o schema do banco e semear dados.
                        using var serviceProvider = services.BuildServiceProvider(true);
                        using var serviceScope = serviceProvider.CreateScope();

                        var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
                        writeDbContext.Database.EnsureCreated();

                        var eventStoreDbContext = serviceScope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
                        eventStoreDbContext.Database.EnsureCreated();

                        // Gancho para inserir dados necessários ao cenário, como usuário autenticado.
                        configureServiceScope?.Invoke(serviceScope);

                        writeDbContext.Dispose();
                        eventStoreDbContext.Dispose();
                    });
                });
        }

        // Cria o HttpClient usado pelos testes para chamar a API em memória.
        public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _factory.CreateClient(options);

        // Libera a aplicação de teste quando o cenário termina.
        public void Dispose() => _factory.Dispose();

        // Versão assíncrona do Dispose para funcionar bem com await using nos testes.
        public async ValueTask DisposeAsync()
        {
            await _factory.DisposeAsync();
        }
    }
}
