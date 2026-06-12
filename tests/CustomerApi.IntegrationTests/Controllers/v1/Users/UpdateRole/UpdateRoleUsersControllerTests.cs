using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using CustomerApi.Application.Users.Commands.Update.Role;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.WebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Users.UpdateRole;

[IntegrationTest]
public class UpdateRoleUsersControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/users/role";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_Put_ValidRequest()
    {
        // Prepara o cenário.
        var user = CreateUser();

        await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
        {
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Users.Add(user);
            writeDbContext.SaveChanges();
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        var command = new UpdateUserRoleCommand
        {
            Id = user.Id,
            Role = UserRole.Admin
        };

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PutAsync(Endpoint, jsonContent);

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
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Put_InvalidRequest()
    {
        // Prepara o cenário.
        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        var command = new UpdateUserRoleCommand
        {
            Id = Guid.Empty,
            Role = (UserRole)999
        };

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PutAsync(Endpoint, jsonContent);

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
    public async Task Should_ReturnsStatus404NotFound_When_Put_NonExistingUser()
    {
        // Prepara o cenário.
        var userId = Guid.NewGuid();

        await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAsAdmin(httpClient);

        var command = new UpdateUserRoleCommand
        {
            Id = userId,
            Role = UserRole.Operator
        };

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PutAsync(Endpoint, jsonContent);

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
            .And.AllSatisfy(error => error.Message.Should().Be($"Nenhum usuario encontrado com o Id: {userId}"));
    }

    private static User CreateUser()
    {
        var faker = new Faker();

        return User.Create(
            faker.Internet.UserName(),
            faker.Person.Email,
            UserRole.Operator,
            faker.Person.FullName,
            faker.Person.DateOfBirth,
            faker.Name.JobTitle(),
            "hashed-password");
    }
}
