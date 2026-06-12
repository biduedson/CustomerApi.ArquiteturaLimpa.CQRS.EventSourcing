using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.WebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Customers.Delete;

[IntegrationTest]
public class DeleteCustomersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/customers";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_Delete_ValidRequest()
    {
        // Prepara o cenário.
        var customer = new Faker<Customer>()
            .CustomInstantiator(faker =>
            {
                var emailResult = Email.Create(faker.Person.Email);
                return Customer.Create(
                    faker.Person.FirstName,
                    faker.Person.LastName,
                    faker.PickRandom<EGender>(),
                    emailResult.Address,
                    faker.Person.DateOfBirth);
            }).Generate();

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
        {
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Customers.Add(customer);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        // Executa a chamada HTTP.
        using var act = await httpClient.DeleteAsync($"{Endpoint}/{customer.Id}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Delete_InvalidRequest()
    {
        // Prepara o cenário.
        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        // Executa a chamada HTTP.
        using var act = await httpClient.DeleteAsync($"{Endpoint}/{Guid.Empty}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Should_Returistatus404NotFound_When_Delete_NonExistingCustomer()
    {
        // Prepara o cenário.
        var customerId = Guid.NewGuid();

        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        // Executa a chamada HTTP.
        using var act = await httpClient.DeleteAsync($"{Endpoint}/{customerId}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Errors.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.AllSatisfy(error => error.Message.Should().Be($"Nenhum cliente encontrado com o Id: {customerId}"));
    }
}
