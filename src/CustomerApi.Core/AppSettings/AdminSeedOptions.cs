using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Core.AppSettings;

public sealed class AdminSeedOptions : IAppOptions
{
    public static string ConfigSectionPath => "AdminSeed";

    public bool Enabled { get; private init; }
    public string Username { get; private init; } = string.Empty;
    public string Email { get; private init; } = string.Empty;
    public string Role { get; private init; } = string.Empty;
    public string FullName { get; private init; } = string.Empty;
    public DateTime? DateOfBirth { get; private init; }
    public string JobTitle { get; private init; } = string.Empty;
    public string Password { get; private init; } = string.Empty;
}
