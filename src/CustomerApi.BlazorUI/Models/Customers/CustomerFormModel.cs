namespace CustomerApi.BlazorUI.Models.Customers;

public sealed class CustomerFormModel
{
    public Guid? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
}