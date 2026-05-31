
using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Settings;

public sealed class CustomerApiSettings : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => "CustomerApi";

    public string? BaseUrl { get; private init; }
}