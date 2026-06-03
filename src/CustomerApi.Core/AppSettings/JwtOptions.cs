using System.ComponentModel.DataAnnotations;
using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Core.AppSettings;

public sealed class JwtOptions : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => "JwtOptions";

    [Required]
    public string Issuer { get; private init; } = string.Empty;

    [Required]
    public string Audience { get; private init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string Secret { get; private init; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationInMinutes { get; private init; }

    [Range(1, 30)]
    public int RefreshTokenExpirationInDays { get; private init; }
}