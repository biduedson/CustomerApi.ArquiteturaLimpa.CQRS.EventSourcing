namespace CustomerApi.WebApi.Models.Account;

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);