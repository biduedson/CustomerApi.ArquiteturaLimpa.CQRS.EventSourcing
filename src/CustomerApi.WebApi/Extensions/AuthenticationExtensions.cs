using System;
using System.Text;
using System.Threading.Tasks;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CustomerApi.WebApi.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
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
}