using CustomerApi.BlazorUI.Models.Customers;
using CustomerApi.BlazorUI.Services.ApiClients;
using CustomerApi.BlazorUI.Services.Authentication;

namespace CustomerApi.BlazorUI.Services.ApiClients.Customers;

public sealed class CustomerApiClient(
    HttpClient httpClient,
    ApiResponseAuthHandler authHandler)
    : BaseApiClient<CreateCustomerRequest, UpdateCustomerRequest, Guid, CustomerListItem>(
        httpClient,
        authHandler,
        Route),
        ICustomerApiClient
{
    private const string Route = "api/customers";
}
