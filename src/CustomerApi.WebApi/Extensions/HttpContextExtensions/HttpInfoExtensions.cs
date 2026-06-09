using System.Linq;
using Microsoft.AspNetCore.Http;
using UAParser;

namespace CustomerApi.WebApi.Extensions.HttpContextExtensions;

public static class HttpInfoExtensions
{
    public static string GetUserAgent(this HttpRequest request)
    {
        var raw = request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var parser = Parser.GetDefault();
        var client = parser.Parse(raw);

        return $"{client.UA.Family} {client.UA.Major} / {client.OS.Family} {client.OS.Major} / {client.Device.Family}";
    }

    public static string? GetIpAddress(this HttpRequest request)
    {
        return request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}