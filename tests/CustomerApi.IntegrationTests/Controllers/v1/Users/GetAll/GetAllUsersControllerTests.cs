using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
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

namespace CustomerApi.IntegrationTests.Controllers.Users.GetAll;

[IntegrationTest]
public class GetAllUsersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/users";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_GetAll()
    {
        // Prepara o cenário.
        var queryModels = new Faker<UserQueryModel>()
            .UsePrivateConstructor()
            .RuleFor(queryModel => queryModel.Id, faker => faker.Random.Guid())
            .RuleFor(queryModel => queryModel.UserName, faker => faker.Internet.UserName())
            .RuleFor(queryModel => queryModel.Email, faker => faker.Person.Email)
            .RuleFor(queryModel => queryModel.Role, faker => faker.PickRandom<UserRole>().ToString())
            .RuleFor(queryModel => queryModel.FullName, faker => faker.Person.FullName)
            .RuleFor(queryModel => queryModel.DateOfBirth, faker => faker.Person.DateOfBirth)
            .RuleFor(queryModel => queryModel.JobTitle, faker => faker.Name.JobTitle())
            .RuleFor(queryModel => queryModel.IsActive, true)
            .Generate(10);

        var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
        readOnlyRepository.GetAllAsync().Returns(queryModels);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IUserReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync(Endpoint);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<IEnumerable<UserQueryModel>>>();
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
                model.UserName.Should().NotBeNullOrWhiteSpace();
                model.Email.Should().NotBeNullOrWhiteSpace();
                model.Role.Should().NotBeNullOrWhiteSpace();
                model.FullName.Should().NotBeNullOrWhiteSpace();
                model.JobTitle.Should().NotBeNullOrWhiteSpace();
                model.IsActive.Should().BeTrue();
            });

        await readOnlyRepository.Received(1).GetAllAsync();
    }
}
