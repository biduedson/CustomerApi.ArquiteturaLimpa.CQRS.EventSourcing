using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate.Events;
using CustomerApi.Domain.Exceptions;
using CustomerApi.Domain.ValueObjects;

namespace CustomerApi.Domain.Entities.UserAggregate;

public class User : BaseEntity, IAggregateRoot
{
    private bool _isDeleted;

    public string UserName { get; } = string.Empty;
    public Email Email { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public UserProfile Profile { get; private set; } = default!;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { }

    private User(string userName, Email email, UserRole role, UserProfile profile, string passwordHash)
    {
        UserName = userName;
        Email = email;
        Role = role;
        Profile = profile;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserCreatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public static User Create(string userName, string email, UserRole role, string fullName, DateTime dateOfBirth, string jobTitle, string passwordHash)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(userName), "O nome de usuário deve ser fornecido.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(passwordHash), "A senha do usuário deve ser fornecida.");

        var emailCreated = Email.Create(email);
        var profileCreated = UserProfile.Create(fullName, dateOfBirth, jobTitle);

        return new User(userName, emailCreated, role, profileCreated, passwordHash);
    }

    public void ChangeProfile(UserProfile newProfile)
    {
        if (newProfile.Equals(Profile))
            return;

        Profile = newProfile;

        MarkAsUpdated();
        AddDomainEvent(new UserUpdatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public void ChangeEmail(Email newEmail)
    {
        if (Email!.Equals(newEmail))
            return;

        Email = newEmail;

        MarkAsUpdated();
        AddDomainEvent(new UserUpdatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;

        MarkAsUpdated();
        AddDomainEvent(new UserUpdatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public void ChangePassword(string newPasswordHash)
    {

        DomainException.ThrowIf(string.IsNullOrWhiteSpace(newPasswordHash), "A nova senha deve ser fornecida.");

        if (PasswordHash == newPasswordHash)
            return;

        PasswordHash = newPasswordHash;

        MarkAsUpdated();
        AddDomainEvent(new UserPasswordChangedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));

    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;

        MarkAsUpdated();
        AddDomainEvent(new UserUpdatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;

        MarkAsUpdated();
        AddDomainEvent(new UserUpdatedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    public void Delete()
    {
        if (_isDeleted) return;

        _isDeleted = true;

        AddDomainEvent(new UserDeletedEvent(
         Id,
         UserName,
         Email.Address,
         Role,
         Profile.FullName,
         Profile.DateOfBirth,
         Profile.JobTitle,
         IsActive,
         CreatedAt,
         UpdatedAt));
    }

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

}