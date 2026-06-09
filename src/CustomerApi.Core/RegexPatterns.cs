using System.Text.RegularExpressions;

namespace CustomerApi.Core;

public static partial class RegexPatterns
{
    public static readonly Regex EmailIsValid = EmailRegexPatternAttr();
    public static readonly Regex PasswordIsValid = PasswordRegexPatternAttr();

    [GeneratedRegex(
        @"^([0-9a-zA-Z]([+\-_.][ 0-9a-zA-Z]+)*)+" +
        @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegexPatternAttr();

    // Regras: entre 8 e 100 chars, pelo menos 1 maiúscula, 1 minúscula, 1 dígito e 1 caractere especial
    [GeneratedRegex(
        @"^(?=.{8,100}$)(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PasswordRegexPatternAttr();
}