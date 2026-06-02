using CustomerApi.Domain.Exceptions;

namespace CustomerApi.Domain.ValueObjects;

public sealed record UserProfile
{
    public string FullName { get; }
    public DateTime DateOfBirth { get; }
    public string JobTitle { get; }

    private UserProfile()
    {
        FullName = string.Empty;
        JobTitle = string.Empty;
    }

    private UserProfile(string fullName, DateTime dateOfBirth, string jobTitle)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        JobTitle = jobTitle;
    }

    public static UserProfile Create(string fullName, DateTime dateOfBirth, string jobTitle)
    {
        DomainException.ThrowIf(dateOfBirth.Date > DateTime.UtcNow.Date.AddYears(-18), "O usuário deve ser maior de  18 anos.");

        DomainException.ThrowIf(string.IsNullOrWhiteSpace(fullName), "O Nome completo do usuário deve ser fornecido.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(jobTitle), "A função do usuário deve ser fornecido.");

        return new UserProfile(fullName.Trim(), dateOfBirth, jobTitle.Trim());
    }
}