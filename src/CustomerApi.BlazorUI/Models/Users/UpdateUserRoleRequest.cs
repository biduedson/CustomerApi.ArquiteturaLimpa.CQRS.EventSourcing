using System.ComponentModel.DataAnnotations;
using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Models.Users;

public sealed class UpdateUserRoleRequest : IRequest
{
    public Guid Id { get; set; }

    [Range(1, 3, ErrorMessage = "Role é obrigatória.")]
    public int Role { get; set; }
}
