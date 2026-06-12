using CustomerApi.Core.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Core;

[UnitTest]
public class JsonExtensionsTests
{
    private const string UserJson =
       "{\"email\":\"john.doe@hotmail.com\",\"userName\":\"John Doe\",\"status\":\"active\"}";

    [Fact]
    public void Should_ReturnJsonString_WhenSerialize()
    {
        // Prepara o cenario.
        var user = new User("John Doe", "john.doe@hotmail.com", EStatus.Active);

        // Executa a acao.
        var act = user.ToJson();

        // Valida o resultado.
        act.Should().NotBeNullOrWhiteSpace().And.BeEquivalentTo(UserJson);
    }

    [Fact]
    public void Should_ReturnEntity_WhenDeserializeTyped()
    {
        // Prepara o cenario.
        var expectedUser = new User("John Doe", "john.doe@hotmail.com", EStatus.Active);

        // Executa a acao.
        var act = UserJson.FromJson<User>();

        // Valida o resultado.
        act.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
        act.UserName.Should().Be(expectedUser.UserName);
        act.Email.Should().NotBeNullOrWhiteSpace();
        act.Status.Should().Be(EStatus.Active);
    }


    [Fact]
    public void Should_ReturnNull_WhenSerializeNullValue()
    {
        // Prepara o cenario.
        User? user = null;

        // Executa a acao.
        var act = user.ToJson();

        // Valida o resultado.
        act.Should().BeNull();
    }

    [Fact]
    public void Should_ReturnNull_WhenDeserializeNullValueTyped()
    {
        // Prepara o cenario.
        const string? strJson = null;

        // Executa a acao.
        var act = strJson?.FromJson<User>();

        // Valida o resultado.
        act.Should().BeNull();
    }
    private enum EStatus
    {
        Active = 0,
        Inactive = 1
    }
    private record User(string UserName, string Email, EStatus Status)
    {
        public string Email { get; } = Email;
        public string UserName { get; } = UserName;
        public EStatus Status { get; } = Status;
    }
}
