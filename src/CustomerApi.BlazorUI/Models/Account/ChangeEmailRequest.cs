using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Account;

public sealed class ChangeEmailRequest
{
    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;
}
