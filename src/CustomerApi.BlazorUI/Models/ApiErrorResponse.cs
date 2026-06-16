
namespace CustomerApi.BlazorUI.Models;

public sealed class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;

    public override string ToString() => Message;
}
