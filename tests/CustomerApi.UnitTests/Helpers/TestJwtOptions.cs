using CustomerApi.Core.AppSettings;
using Microsoft.Extensions.Options;

namespace CustomerApi.UnitTests.Helpers;

internal static class TestJwtOptions
{
    public static IOptions<JwtOptions> Create()
    {
        var jwtOptions = new JwtOptions();

        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Issuer))!.SetValue(jwtOptions, "CustomerApi");
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Audience))!.SetValue(jwtOptions, "CustomerApi.Tests");
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.Secret))!.SetValue(jwtOptions, new string('x', 64));
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.AccessTokenExpirationInMinutes))!.SetValue(jwtOptions, 15);
        typeof(JwtOptions).GetProperty(nameof(JwtOptions.RefreshTokenExpirationInDays))!.SetValue(jwtOptions, 7);

        return Options.Create(jwtOptions);
    }
}
