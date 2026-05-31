namespace CustomerApi.BlazorUI.Models.Auth;

public sealed class LoginFormModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}