using CustomerApi.BlazorUI.Components;
using CustomerApi.BlazorUI.Extensions.EndpointRouteBuilderExtensions;
using CustomerApi.BlazorUI.Extensions.ServiceCollectionsExtensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services
                .ConfigureAppSettings()
                .AddHttpContextAccessor()
                .AddCustomerApiHttpClients(builder.Configuration)
                .AddCustomerApiServices()
                .AddCustomerApiAuthentication(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
