using CustomerApi.Core;
using CustomerApi.Domain.Exceptions;

namespace CustomerApi.Domain.ValueObjects;

public sealed record Password
{
    private Password(string password) => Value = password;
    public string Value { get; } = default!;

    public Password() { }

    public static Password Create(string password)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(password), "O password deve ser fornecido.");
        DomainException.ThrowIf(!RegexPatterns.PasswordIsValid.IsMatch(password), "O password  esta com formato inválido.");
        return new Password(password);
    }

    public override string? ToString() => Value;
}
