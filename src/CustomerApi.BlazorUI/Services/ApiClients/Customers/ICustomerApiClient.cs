using CustomerApi.BlazorUI.Abstractions.ApiClients;
using CustomerApi.BlazorUI.Models.Customers;

namespace CustomerApi.BlazorUI.Services.ApiClients.Customers;

public interface ICustomerApiClient
    : IApiClient<CreateCustomerRequest, UpdateCustomerRequest, Guid, CustomerListItem>
{
}
