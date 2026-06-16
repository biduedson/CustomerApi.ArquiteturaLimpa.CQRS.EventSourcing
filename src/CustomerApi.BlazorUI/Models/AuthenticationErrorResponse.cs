namespace CustomerApi.BlazorUI.Models;

public sealed class AuthenticationErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
