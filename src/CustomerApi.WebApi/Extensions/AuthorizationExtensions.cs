using CustomerApi.Domain.Entities.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.WebApi.Extensions;

public static class AuthorizationExtensions
{
    public const string ViewerOrAbove = nameof(ViewerOrAbove);
    public const string OperatorOrAdmin = nameof(OperatorOrAdmin);
    public const string AdminOnly = nameof(AdminOnly);

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
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