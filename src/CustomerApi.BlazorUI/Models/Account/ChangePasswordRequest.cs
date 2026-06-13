using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Account;

public sealed class ChangePasswordRequest : IValidatableObject
{
    [Required(ErrorMessage = "Senha atual é obrigatória.")]
    [MaxLength(100, ErrorMessage = "Senha atual deve ter no máximo 100 caracteres.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória.")]
    [MaxLength(100, ErrorMessage = "Nova senha deve ter no máximo 100 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação da senha é obrigatória.")]
    [MaxLength(100, ErrorMessage = "Confirmação da senha deve ter no máximo 100 caracteres.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "A confirmação da nova senha não corresponde à nova senha informada.",
                [nameof(ConfirmPassword)]);
        }
    }
}
