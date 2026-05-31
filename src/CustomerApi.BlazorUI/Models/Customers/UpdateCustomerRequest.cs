namespace CustomerApi.BlazorUI.Models.Customers;

public sealed class UpdateCustomerRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

