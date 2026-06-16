using CustomerApi.BlazorUI.Abstractions;

namespace CustomerApi.BlazorUI.Models.Customers;

public sealed class CreateCustomerRequest : IRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
