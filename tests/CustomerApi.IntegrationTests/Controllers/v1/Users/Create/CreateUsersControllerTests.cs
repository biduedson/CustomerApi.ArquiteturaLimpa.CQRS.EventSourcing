using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Application.Users.Responses;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.WebApi.Models;
using CustomerApi.WebApi.Models.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Users.Create;

[IntegrationTest]
public class CreateUsersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/users";

    [Fact]
    public async Task Should_ReturnsHttpStatus201Created_When_Post_ValidRequest()
    {
        // Prepara o cenário.
        var authenticatedUser = CreateUser(role: UserRole.Admin);

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
        {
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Users.Add(authenticatedUser);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient, authenticatedUser.Id);

        var request = CreateUserDto();

        using var jsonContent = request.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreateUserResponse>>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNull();
        response.Result.Id.Should().NotBeEmpty();

        act.Headers.GetValues("Location").Should().NotBeNullOrEmpty()
            .And.Contain($"/api/users/{response.Result.Id}");
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Post_InvalidRequest()
    {
        // Prepara o cenário.
        var authenticatedUser = CreateUser(role: UserRole.Admin);

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
        {
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Users.Add(authenticatedUser);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient, authenticatedUser.Id);

        var request = new CreateUserDto(
            string.Empty,
            string.Empty,
            default,
            string.Empty,
            default,
            string.Empty,
            string.Empty);

        using var jsonContent = request.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreateUserResponse>>();
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
        var authenticatedUser = CreateUser(role: UserRole.Admin);
        var existingUser = CreateUser();

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
        {
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Users.Add(authenticatedUser);
            writeDbContext.Users.Add(existingUser);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient, authenticatedUser.Id);

        var request = CreateUserDto(email: existingUser.Email.Address);

        using var jsonContent = request.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreateUserResponse>>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Result.Should().BeNull();
        response.Errors.Should().NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.AllSatisfy(error => error.Message.Should().Be("O endereço de e-mail informado já está em uso."));
    }

    private static CreateUserDto CreateUserDto(string email = null)
    {
        var faker = new Faker();

        return new CreateUserDto(
            faker.Internet.UserName(),
            email ?? faker.Person.Email,
            UserRole.Operator,
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            faker.Name.JobTitle(),
            CreateValidFakePassword());
    }

    private static string CreateValidFakePassword() => $"{nameof(CreateValidFakePassword)}1!";

    private static User CreateUser(UserRole role = UserRole.Operator)
    {
        var faker = new Faker();

        return User.Create(
            faker.Internet.UserName(),
            faker.Person.Email,
            role,
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            faker.Name.JobTitle(),
            "hashed-password");
    }
}
