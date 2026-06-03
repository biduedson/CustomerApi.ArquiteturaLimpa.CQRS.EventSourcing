using CustomerApi.Core.SharedKernel;

namespace CustomerApi.Domain.Entities.UserAggregate.Events;

public class UserBaseEvent : BaseEvent
{
    protected UserBaseEvent(
        Guid id,
        string userName,
        string email,
        UserRole role,
        string fullName,
        DateTime dateOfBirth,
        string jobTitle,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt
    )
    {
        Id = id;
        AggregateId = id;
        UserName = userName;
        Email = email;
        Role = role;
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        JobTitle = jobTitle;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    public Guid Id { get; private init; }
    public string UserName { get; private init; }
    public string Email { get; private init; }
    public UserRole Role { get; private init; }
    public string FullName { get; private init; }
    public DateTime DateOfBirth { get; private init; }
    public string JobTitle { get; private init; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private init; }
}