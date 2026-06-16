using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;
using CustomerApi.WebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Customers.GetById;

[IntegrationTest]
public class GetByIdCustomersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/customers";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_GetById_ValidRequest()
    {
        // Prepara o cenário.
        var queryModel = new Faker<CustomerQueryModel>()
            .UsePrivateConstructor()
            .RuleFor(queryModel => queryModel.Id, faker => faker.Random.Guid())
            .RuleFor(queryModel => queryModel.FirstName, faker => faker.Person.FirstName)
            .RuleFor(queryModel => queryModel.LastName, faker => faker.Person.LastName)
            .RuleFor(queryModel => queryModel.Email, faker => faker.Person.Email)
            .RuleFor(queryModel => queryModel.Gender, faker => faker.PickRandom<EGender>().ToString())
            .RuleFor(queryModel => queryModel.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate();

        var readOnlyRepository = Substitute.For<ICustomerReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(queryModel);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<ICustomerReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync($"{Endpoint}/{queryModel.Id}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CustomerQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNull();

        response.Result.Id.Should().NotBeEmpty().And.Be(queryModel.Id);
        response.Result.FirstName.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.FirstName);
        response.Result.LastName.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.LastName);
        response.Result.Email.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Email);
        response.Result.Gender.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Gender);
        response.Result.DateOfBirth.Should().Be(queryModel.DateOfBirth);
        response.Result.FullName.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.FullName);

        await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == queryModel.Id));
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_GetById_InvalidRequest()
    {
        // Prepara o cenário.
        var readOnlyRepository = Substitute.For<ICustomerReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((CustomerQueryModel)null);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<ICustomerReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var customerId = Guid.Empty;

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync($"{Endpoint}/{customerId}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CustomerQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();

        await readOnlyRepository.DidNotReceive().GetByIdAsync(Arg.Is<Guid>(id => id == customerId));
    }

    [Fact]
    public async Task Should_ReturnsStatus404NotFound_When_GetById_NonExistingCustomer()
    {
        // Prepara o cenário.
        var readOnlyRepository = Substitute.For<ICustomerReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((CustomerQueryModel)null);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<ICustomerReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var customerId = Guid.NewGuid();

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync($"{Endpoint}/{customerId}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CustomerQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.AllSatisfy(error => error.Message.Should().Be($"Nenhum cliente encontrado com o Id: {customerId}"));

        await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == customerId));
    }
}
