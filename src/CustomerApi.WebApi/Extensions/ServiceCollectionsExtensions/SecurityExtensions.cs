using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CustomerApi.WebApi.Extensions.ServiceCollectionsExtensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddJwtAuthentication(configuration)
                .AddAuthorizationPolicies();
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetOptions<JwtOptions>();

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions!.Secret));

        services
           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.RequireHttpsMetadata = true;
               options.SaveToken = false;

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = jwtOptions.Issuer,

                   ValidateAudience = true,
                   ValidAudience = jwtOptions.Audience,

                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = signingKey,

                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.Zero
               };

               options.Events = new JwtBearerEvents
               {
                   OnMessageReceived = context =>
                   {
                       context.Token = context.Request.Cookies["access_Token"];
                       return Task.CompletedTask;
                   },
                   OnAuthenticationFailed = context =>
                   {
                       return Task.CompletedTask;
                   },
                   OnChallenge = context =>
                   {
                       context.HandleResponse();

                       if (context.AuthenticateFailure is SecurityTokenExpiredException)
                       {
                           return WriteAuthenticationErrorAsync(
                               context.Response,
                               StatusCodes.Status401Unauthorized,
                               "Token de acesso expirado.",
                               "O token de acesso enviado expirou.",
                               "ACCESS_TOKEN_EXPIRED");
                       }

                       if (string.IsNullOrWhiteSpace(context.Request.Cookies["access_Token"]))
                       {
                           return WriteAuthenticationErrorAsync(
                               context.Response,
                               StatusCodes.Status401Unauthorized,
                               "Token de acesso ausente.",
                               "Nenhum token de acesso foi enviado.",
                               "ACCESS_TOKEN_MISSING");
                       }

                       return WriteAuthenticationErrorAsync(
                           context.Response,
                           StatusCodes.Status401Unauthorized,
                           "Token de acesso inválido.",
                           "O token de acesso enviado é inválido.",
                           "ACCESS_TOKEN_INVALID");
                   },
                   OnForbidden = context =>
                       WriteAuthenticationErrorAsync(
                           context.Response,
                           StatusCodes.Status403Forbidden,
                           "Acesso negado.",
                           "Você não possui permissão para acessar este recurso.",
                           "ACCESS_FORBIDDEN")
               };

           });

        return services;

    }

    public const string ViewerOrAbove = nameof(ViewerOrAbove);
    public const string OperatorOrAdmin = nameof(OperatorOrAdmin);
    public const string AdminOnly = nameof(AdminOnly);

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
            {
                options.AddPolicy(ViewerOrAbove, policy =>
                    policy.RequireRole(
                        nameof(UserRole.Viewer),
                        nameof(UserRole.Operator),
                        nameof(UserRole.Admin)));

                options.AddPolicy(OperatorOrAdmin, policy =>
                     policy.RequireRole(
                        nameof(UserRole.Operator),
                        nameof(UserRole.Admin)));

                options.AddPolicy(AdminOnly, policy =>
                     policy.RequireRole(
                        nameof(UserRole.Admin)));

            });

        return services;
    }

    private static Task WriteAuthenticationErrorAsync(
        HttpResponse response,
        int statusCode,
        string title,
        string detail,
        string errorCode)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        var error = new AuthenticationErrorResponse(title, statusCode, detail, errorCode);
        var json = JsonSerializer.Serialize(error, JsonOptions);

        return response.WriteAsync(json);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record AuthenticationErrorResponse(
        string Title,
        int Status,
        string Detail,
        string ErrorCode);
}
