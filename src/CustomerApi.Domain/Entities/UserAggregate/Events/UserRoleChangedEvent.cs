namespace CustomerApi.Domain.Entities.UserAggregate.Events;

public class UserRoleChangedEvent(
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
    ) : UserBaseEvent(id, userName, email, role, fullName, dateOfBirth, jobTitle, isActive, createdAt, updatedAt);