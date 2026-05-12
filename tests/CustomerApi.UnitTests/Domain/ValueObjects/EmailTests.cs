using CustomerApi.Domain.Exceptions;
using CustomerApi.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Domain.ValueObjects;

[UnitTest]
public class EmailTests
{
    [Theory]
    [InlineData("ma@hostname.com")]
    [InlineData("ma@hostname.comcom")]
    [InlineData("MA@hostname.coMCom")]
    [InlineData("MA@HOSTNAME.COM")]
    [InlineData("m.a@hostname.co")]
    [InlineData("m_a1a@hostname.com")]
    [InlineData("ma-a@hostname.com")]
    [InlineData("ma-a@hostname.com.edu")]
    [InlineData("ma-a.aa@hostname.com.edu")]
    [InlineData("ma.h.saraf.onemore@hostname.com.edu")]
    [InlineData("ma12@hostname.com")]
    [InlineData("12@hostname.com")]
    public void Should_ReturnsSuccess_When_CreateEmailIsValid(string emailAddress)
    {
        var act = Email.Create(emailAddress);

        act.Should().NotBeNull();
        act.Should().NotBeNull().And.BeOfType<Email>();
        act.Address.Should().NotBeNullOrEmpty().And.Be(emailAddress.ToLowerInvariant());
    }

    [Theory]
    [InlineData("Abc.example.com")]     // E-mail sem o símbolo @
    [InlineData("A@b@c@example.com")]   // Múltiplos símbolos @
    [InlineData("ma...ma@jjf.co")]      // Múltiplos pontos seguidos no nome do usuário
    [InlineData("ma@jjf.c")]            // Extensão do domínio com apenas 1 caractere
    [InlineData("ma@jjf..com")]         // Múltiplos pontos seguidos no domínio
    [InlineData("ma@@jjf.com")]         // Múltiplos @ seguidos
    [InlineData("@majjf.com")]          // Nada antes do @
    [InlineData("ma.@jjf.com")]         // Nada após o ponto
    [InlineData("ma_@jjf.com")]         // Nada após o underscore
    [InlineData("ma_@jjf")]             // Sem extensão de domínio
    [InlineData("ma_@jjf.")]            // Nada após _ e ponto
    [InlineData("ma@jjf.")]             // Nada após o ponto final
    public void Should_ReturnsFail_When_CreateEmailInvalid(string emailAddress) 
    {
        var act = () => Email.Create(emailAddress);

        act.Should().ThrowExactly<DomainException>()
        .WithMessage("O endereço de e-mail é inválido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_ReturnsFail_When_CreateEmailIsEmptyOrNull(string? emailAddress)
    {
        var act = () => Email.Create(emailAddress!);
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("O endereço de e-mail deve ser fornecido.");
    }
}
