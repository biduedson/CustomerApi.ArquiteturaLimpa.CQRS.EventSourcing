namespace CustomerApi.BlazorUI.Models.Users;

public sealed class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
