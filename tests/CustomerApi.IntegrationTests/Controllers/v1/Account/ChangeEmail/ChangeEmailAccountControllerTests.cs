using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CustomerApi.Application.Account.Commands.Emails.Change;
using CustomerApi.Core.Extensions;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.IntegrationTests.Extensions;
using CustomerApi.WebApi.Models;
using CustomerApi.WebApi.Models.Account;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.IntegrationTests.Controllers.Account.ChangeEmail;

[IntegrationTest]
public class ChangeEmailAccountControllerTests : ControllerTestsBase
{
    private const string Endpoint = "/api/account/changeemail";

    [Fact]
    public async Task Should_ReturnsHttpStatus200Ok_When_Post_ValidRequest()
    {
        // Prepara o cenário.
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ChangeEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IMediator>();
            services.AddScoped(_ => mediator);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator, userId);

        var request = new ChangeEmailDto("new.email@test.com");

        using var jsonContent = request.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Valida a resposta.
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.OK);
        act.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies.Should().Contain(cookie =>
            cookie.Contains("access_Token=", StringComparison.Ordinal)
            && cookie.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase));
        cookies.Should().Contain(cookie =>
            cookie.Contains("refresh_Token=", StringComparison.Ordinal)
            && cookie.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase));

        await mediator.Received(1).Send(
            Arg.Is<ChangeEmailCommand>(command =>
                command.UserId == userId
                && command.Email == request.Email),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnsHttpStatus400BadRequest_When_Post_InvalidRequest()
    {
        // Prepara o cenário.
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ChangeEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error("O e-mail informado esta com formato invalido."));

        await using var webApplicationFactory = InitializeWebAppFactory(services =>
        {
            services.RemoveAll<IMediator>();
            services.AddScoped(_ => mediator);
        });

        using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());
        AuthenticateAs(httpClient, UserRole.Operator);

        var request = new ChangeEmailDto("invalid-email");

        using var jsonContent = request.ToJsonHttpContent();
        // Executa a chamada HTTP.
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

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
}
