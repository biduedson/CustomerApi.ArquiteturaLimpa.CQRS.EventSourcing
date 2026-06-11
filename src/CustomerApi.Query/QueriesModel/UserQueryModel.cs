using System;
using CustomerApi.Query.Abstractions;

namespace CustomerApi.Query.QueriesModel;

public class UserQueryModel : IQueryModel<Guid>
{
    public UserQueryModel(
     Guid id,
     string userName,
     string email,
     string role,
     string fullName,
     DateTime dateOfBirth,
     string jobTitle,
     bool isActive
)
    {
        Id = id;
        UserName = userName;
        Email = email;
        Role = role;
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        JobTitle = jobTitle;
        IsActive = isActive;
    }

    private UserQueryModel()
    {
    }
    public Guid Id { get; private init; }
    public string UserName { get; private init; }
    public string Email { get; private init; }
    public string Role { get; private init; }
    public string FullName { get; private init; }
    public DateTime DateOfBirth { get; private init; }
    public string JobTitle { get; private init; }
    public bool IsActive { get; private init; }
}