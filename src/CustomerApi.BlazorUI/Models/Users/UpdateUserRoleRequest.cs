using System.ComponentModel.DataAnnotations;

namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UpdateUserRoleRequest
{
    public Guid Id { get; set; }

    [Range(1, 3, ErrorMessage = "Role é obrigatória.")]
    public int Role { get; set; }
}
