namespace CustomerApi.BlazorUI.Models;

public class ApiResponse<TResult>
{
    public TResult? Result { get; set; }
    public bool Success { get; set; }
    public string? SucessMessage { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = [];
}