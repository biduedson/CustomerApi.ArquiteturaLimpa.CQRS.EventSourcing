using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UserProfileFormModel
{
    public Guid Id { get; set; }

    [MaxLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Cargo é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Cargo deve ter no máximo 100 caracteres.")]
    public string JobTitle { get; set; } = string.Empty;
}
