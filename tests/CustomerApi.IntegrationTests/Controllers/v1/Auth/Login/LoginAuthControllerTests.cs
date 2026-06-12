using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CustomerApi.Application.Auth.Commands.Login;
using CustomerApi.Application.Auth.Responses;
using CustomerApi.Core.Extensions;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.WebApi.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Auth.Login;

[IntegrationTest]
public class LoginAuthControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/auth/login";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_Post_ValidRequest()
    {
        // Prepara o cenário.
        var authentication = CreateAuthenticationResponse();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticationResponse>.Success(authentication));

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IMediator>();
            services.AddScoped(_ => mediator);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CustomerApi.IntegrationTests");

        var command = new LoginCommand
        {
            Email = "admin@test.com",
            Password = "fake-login-password"
        };

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);
        act.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies.Should().Contain(cookie => cookie.Contains("access_Token=", StringComparison.Ordinal));
        cookies.Should().Contain(cookie => cookie.Contains("refresh_Token=", StringComparison.Ordinal));

        await mediator.Received(1).Send(
            Arg.Is<LoginCommand>(request =>
                request.Email == command.Email
                && request.Password == command.Password
                && !string.IsNullOrWhiteSpace(request.UserAgent)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Post_InvalidCredentials()
    {
        // Prepara o cenário.
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticationResponse>.Error("Credenciais invalidas."));

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IMediator>();
            services.AddScoped(_ => mediator);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

        var command = new LoginCommand
        {
            Email = "admin@test.com",
            Password = "wrong-password"
        };

        using var jsonContent = command.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeFalse();
        act.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        act.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeFalse();

        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
    }

    private static AuthenticationResponse CreateAuthenticationResponse() =>
        new(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));
}
