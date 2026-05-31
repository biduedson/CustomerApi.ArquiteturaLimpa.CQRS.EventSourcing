namespace CustomerApi.BlazorUI.Models;

public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string? SucessMessage { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = [];
}