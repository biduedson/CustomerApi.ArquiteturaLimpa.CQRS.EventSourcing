using System.ComponentModel.DataAnnotations;
using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Models.Account;

public sealed class ChangeEmailRequest : IRequest
{
    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;
}
