namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UpdateUserProfileRequest
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string JobTitle { get; set; } = string.Empty;
}
