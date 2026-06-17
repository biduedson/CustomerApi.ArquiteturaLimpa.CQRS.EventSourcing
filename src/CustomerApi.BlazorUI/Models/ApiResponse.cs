
namespace CustomerApi.BlazorUI.Models;

public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string? SuccessMessage { get; set; }
    public string? SucessMessage { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public List<ApiErrorResponse> Errors { get; set; } = [];

    public List<string> ErrorMessages => Errors
        .Select(error => error.Message)
        .Where(message => !string.IsNullOrWhiteSpace(message))
        .ToList();
}
