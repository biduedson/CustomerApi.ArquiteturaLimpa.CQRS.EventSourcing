using System.Threading.Tasks;
using System.Net;
using Bogus;
using CustomerApi.Application.Customers.Commands.Create;
using CustomerApi.Application.Customers.Responses;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.WebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Customers.Create;

[IntegrationTest]
public class CreateCustomersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/customers";

    [Fact]
    public async Task Should_ReturnsHttpStatus201Created_When_Post_ValidRequest()
    {
        // Prepara o cenário.
        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var command = new Faker<CreateCustomerCommand>()
            .RuleFor(command => command.FirstName, faker => faker.Person.FirstName)
            .RuleFor(command => command.LastName, faker => faker.Person.LastName)
            .RuleFor(command => command.Email, faker => faker.Person.Email)
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate();

        using var jsoncontent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsoncontent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedCustomerResponse>>();

        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNull();
        response.Result.Id.Should().NotBeEmpty();

        act.Headers.GetValues("Location").Should().NotBeNullOrEmpty()
            .And.Contain($"/api/customers/{response.Result.Id}");
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Post_InvalidRequest()
    {
        // Prepara o cenário.
        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var command = new Faker<CreateCustomerCommand>().Generate();

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedCustomerResponse>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Post_EmailAddressIsAlready()
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

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceSope =>
        {
            var writeDbContext = serviceSope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Customers.Add(customer);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var command = new Faker<CreateCustomerCommand>()
            .RuleFor(command => command.FirstName, faker => faker.Person.FirstName)
            .RuleFor(command => command.LastName, faker => faker.Person.LastName)
            .RuleFor(command => command.Email, customer.Email.Address)
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate();

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedCustomerResponse>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.AllSatisfy(error => error.Message.Should().Be("O endereço de e-mail informado já está em uso."));
    }
}
