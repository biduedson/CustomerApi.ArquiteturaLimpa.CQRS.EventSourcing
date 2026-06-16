
namespace CustomerApi.BlazorUI.Models;

public class ApiResponse<TResult>
{
    public TResult? Result { get; set; }
    public bool Success { get; set; }
    public string? SuccessMessage { get; set; }
    public string? SucessMessage { get; set; }
    public int StatusCode { get; set; }
    public List<ApiErrorResponse> Errors { get; set; } = [];

    public List<string> ErrorMessages => Errors
        .Select(error => error.Message)
        .Where(message => !string.IsNullOrWhiteSpace(message))
        .ToList();
}
