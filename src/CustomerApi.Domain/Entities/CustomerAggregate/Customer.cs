using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.CustomerAggregate.Events;
using CustomerApi.Domain.Exceptions;
using CustomerApi.Domain.ValueObjects;

namespace CustomerApi.Domain.Entities.CustomerAggregate;

public class Customer : BaseEntity, IAggregateRoot
{
    public bool _isDeleted;

    private Customer(string firstName, string lastName, EGender gender, Email email, DateTime dateOfBirth) 
    {
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        Email = email;
        DateOfBirth = dateOfBirth;
        AddDomainEvent(new CustomerCreatedEvent(Id, firstName, lastName, gender, email.Address!, dateOfBirth));
        
    }

    public Customer() { }

    public string  FirstName { get;} = string.Empty;
    public string LastName { get; } = string.Empty;
    public EGender Gender { get; }
    public Email? Email { get; private set; } 
    public DateTime DateOfBirth { get;}

    public static Customer Create(string firstName, string lastName, EGender gender, string email, DateTime dateOfBirth)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(firstName), "O FirstName deve ser fornecido.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(lastName), "O LastName deve ser fornecido.");
        var emailCreated = Email.Create(email);

        return new Customer(firstName, lastName, gender, emailCreated, dateOfBirth);
    }

    public void ChangeEmail(Email newEmail)
    {
        if (Email!.Equals(newEmail))
            return;

        Email = newEmail;

        AddDomainEvent(new CustomerUpdatedEvent(Id, FirstName, LastName, Gender, newEmail.Address!, DateOfBirth));
    }

    public void Delete()
    {
        if (_isDeleted) return;

        _isDeleted = true;

        AddDomainEvent(new CustomerDeletedEvent(Id, FirstName, LastName, Gender, Email!.Address!, DateOfBirth));
    }
}
