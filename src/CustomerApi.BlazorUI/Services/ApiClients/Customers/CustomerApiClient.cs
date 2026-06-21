using CustomerApi.BlazorUI.Models.Customers;
using CustomerApi.BlazorUI.Services.Authentication;
using Microsoft.AspNetCore.Components;

namespace CustomerApi.BlazorUI.Services.ApiClients.Customers;

public sealed class CustomerApiClient(
    HttpClient httpClient,
    AuthRefreshService authRefreshService,
    NavigationManager navigation)
    : BaseApiClient<CreateCustomerRequest, UpdateCustomerRequest, Guid, CustomerListItem>(
        httpClient,
        authRefreshService,
        navigation,
        Route),
        ICustomerApiClient
{
    private const string Route = "api/customers";
}
