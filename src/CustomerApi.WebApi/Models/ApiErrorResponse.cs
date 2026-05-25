
using System.Text.Json.Serialization;

namespace CustomerApi.WebApi.Models;

[method: JsonConstructor]
public class ApiErrorResponse(string message)
{
    public string Message { get; } = message;

    public override string ToString() => Message;
}
