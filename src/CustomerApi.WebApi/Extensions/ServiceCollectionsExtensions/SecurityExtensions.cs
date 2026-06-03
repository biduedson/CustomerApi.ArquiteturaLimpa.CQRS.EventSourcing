using System;
using System.Text;
using System.Threading.Tasks;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                       context.Token = context.Request.Cookies["access_token"];

                       return Task.CompletedTask;
                   }
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
}
