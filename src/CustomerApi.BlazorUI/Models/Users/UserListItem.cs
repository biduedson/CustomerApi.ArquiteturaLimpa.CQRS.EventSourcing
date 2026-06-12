namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UserListItem
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
