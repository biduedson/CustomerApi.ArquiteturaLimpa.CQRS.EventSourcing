using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UserCreateFormModel
{
    [Required(ErrorMessage = "Username é obrigatório.")]
    [MaxLength(50, ErrorMessage = "Username deve ter no máximo 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(254, ErrorMessage = "Email deve ter no máximo 254 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "Role é obrigatória.")]
    public int Role { get; set; } = 1;

    [Required(ErrorMessage = "Nome completo é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Cargo é obrigatório.")]
    [MaxLength(100, ErrorMessage = "Cargo deve ter no máximo 100 caracteres.")]
    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha inicial é obrigatória.")]
    [MaxLength(100, ErrorMessage = "Senha deve ter no máximo 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
