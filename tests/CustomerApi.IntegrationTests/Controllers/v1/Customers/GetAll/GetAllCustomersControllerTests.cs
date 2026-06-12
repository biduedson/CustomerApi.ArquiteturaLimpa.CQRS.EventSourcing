using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.CustomerAggregate;
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

namespace CustomerApi.IntegrationTests.Controllers.Customers.GetAll;

[IntegrationTest]
public class GetAllCustomersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/customers";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_GetAll()
    {
        // Prepara o cenário.
        var queryModels = new Faker<CustomerQueryModel>()
            .UsePrivateConstructor()
            .RuleFor(queryModel => queryModel.Id, faker => faker.Random.Guid())
            .RuleFor(queryModel => queryModel.FirstName, faker => faker.Person.FirstName)
            .RuleFor(queryModel => queryModel.LastName, faker => faker.Person.LastName)
            .RuleFor(queryModel => queryModel.Email, faker => faker.Person.Email)
            .RuleFor(queryModel => queryModel.Gender, faker => faker.PickRandom<EGender>().ToString())
            .RuleFor(queryModel => queryModel.DateOfBirth, faker => faker.Person.DateOfBirth)
            .Generate(10);

        var readOnlyRepository = Substitute.For<ICustomerReadOnlyRepository>();
        readOnlyRepository.GetAllAsync().Returns(queryModels);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<ICustomerReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync(Endpoint);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<IEnumerable<CustomerQueryModel>>>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.HaveCount(queryModels.Count)
            .And.AllSatisfy(model =>
            {
                model.Id.Should().NotBeEmpty();
                model.FirstName.Should().NotBeNullOrWhiteSpace();
                model.LastName.Should().NotBeNullOrWhiteSpace();
                model.Email.Should().NotBeNullOrWhiteSpace();
                model.Gender.Should().NotBeNullOrWhiteSpace();
                model.FullName.Should().NotBeNullOrWhiteSpace();
            });

        await readOnlyRepository.Received(1).GetAllAsync();
    }
}
