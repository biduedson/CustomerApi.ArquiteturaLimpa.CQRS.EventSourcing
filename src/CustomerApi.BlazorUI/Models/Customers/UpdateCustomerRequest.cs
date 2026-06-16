using System.ComponentModel.DataAnnotations;
using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Models.Customers;

public sealed class UpdateCustomerRequest : IRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;
}

