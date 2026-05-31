namespace CustomerApi.BlazorUI.Services;

public sealed class ToastService
{
    public event Action<string, string>? OnShow;

    public void Success(string message) => OnShow?.Invoke("success", message);
    public void Error(string message) => OnShow?.Invoke("danger", message);
    public void Info(string message) => OnShow?.Invoke("info", message);
}