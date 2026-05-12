using CustomerApi.Core;
using CustomerApi.Domain.Exceptions;

namespace CustomerApi.Domain.ValueObjects;

public sealed record Email
{
    private Email(string addres) =>
        Address = addres.ToLowerInvariant().Trim();

    public Email() { } 

    public string Address { get; }

    public static Email Create(string emailAddress)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(emailAddress), "O endereço de e-mail deve ser fornecido.");
        DomainException.ThrowIf(!RegexPatterns.EmailIsValid.IsMatch(emailAddress), "O endereço de e-mail é inválido.");
        return new Email(emailAddress);   
    }

    public override string ToString() => Address;
}
