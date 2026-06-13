using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Customers;

public sealed class CustomerFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Sobrenome deve ter no máximo 100 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gênero é obrigatório.")]
    public string Gender { get; set; } = "Male";

    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
    public DateTime? DateOfBirth { get; set; }
}
