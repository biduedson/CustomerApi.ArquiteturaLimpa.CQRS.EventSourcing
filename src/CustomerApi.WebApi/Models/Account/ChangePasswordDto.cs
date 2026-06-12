namespace CustomerApi.WebApi.Models.Account;

public sealed record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
