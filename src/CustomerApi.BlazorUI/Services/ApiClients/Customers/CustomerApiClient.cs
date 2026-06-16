using CustomerApi.BlazorUI.Models.Customers;
using CustomerApi.BlazorUI.Services.ApiClients;

namespace CustomerApi.BlazorUI.Services.ApiClients.Customers;

public sealed class CustomerApiClient(HttpClient httpClient)
    : BaseApiClient<CreateCustomerRequest, UpdateCustomerRequest, Guid, CustomerListItem>(httpClient, Route),
        ICustomerApiClient
{
    private const string Route = "api/customers";
}
