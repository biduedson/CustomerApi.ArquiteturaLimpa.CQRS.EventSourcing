using System;
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

namespace CustomerApi.IntegrationTests.Controllers.Users.GetById;

[IntegrationTest]
public class GetByIdUsersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/users";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_GetById_ValidRequest()
    {
        // Prepara o cenário.
        var queryModel = CreateUserQueryModel();

        var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(queryModel);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IUserReadOnlyRepository>();
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

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNull();

        response.Result.Id.Should().NotBeEmpty().And.Be(queryModel.Id);
        response.Result.UserName.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.UserName);
        response.Result.Email.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Email);
        response.Result.Role.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Role);
        response.Result.FullName.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.FullName);
        response.Result.DateOfBirth.Should().Be(queryModel.DateOfBirth);
        response.Result.JobTitle.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.JobTitle);
        response.Result.IsActive.Should().Be(queryModel.IsActive);

        await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == queryModel.Id));
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_GetById_InvalidRequest()
    {
        // Prepara o cenário.
        var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((UserQueryModel)null);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IUserReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var userId = Guid.Empty;

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync($"{Endpoint}/{userId}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();

        await readOnlyRepository.DidNotReceive().GetByIdAsync(Arg.Is<Guid>(id => id == userId));
    }

    [Fact]
    public async Task Should_ReturnsStatus404NotFound_When_GetById_NonExistingUser()
    {
        // Prepara o cenário.
        var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
        readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((UserQueryModel)null);

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IUserReadOnlyRepository>();
            services.AddScoped(_ => readOnlyRepository);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var userId = Guid.NewGuid();

        // Executa a chamada HTTP.
        using var act = await httpClient.GetAsync($"{Endpoint}/{userId}");

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.AllSatisfy(error => error.Message.Should().Be($"Nenhum usuario encontrado com o Id: {userId}"));

        await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == userId));
    }

    private static UserQueryModel CreateUserQueryModel()
    {
        var faker = new Faker();

        return new UserQueryModel(
            faker.Random.Guid(),
            faker.Internet.UserName(),
            faker.Person.Email,
            faker.PickRandom<UserRole>().ToString(),
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            faker.Name.JobTitle(),
            true);
    }
}
