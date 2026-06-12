using System;
using Bogus;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.Entities.UserAggregate.Events;
using CustomerApi.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Domain.Entities.UserAggregate;

[UnitTest]
public class UserTests
{
    [Fact]
    public void Should_UserCreatedEvent_WhenCreate()
    {
        var act = CreateUser();

        act.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserCreatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeEmail()
    {
        var user = CreateUser();

        user.ChangeEmail(CreateEmail());

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeProfile()
    {
        var user = CreateUser();

        user.ChangeProfile(CreateUserProfile());

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeRole()
    {
        var user = CreateUser();

        user.ChangeRole(UserRole.Admin);

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserPasswordChangedEvent_WhenChangePassword()
    {
        var user = CreateUser();

        user.ChangePassword("new-password-hash");

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserPasswordChangedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenDeactivate()
    {
        var user = CreateUser();

        user.Deactivate();

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenActivate()
    {
        var user = CreateUser();

        user.Deactivate();
        user.Activate();

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserDeletedEvent_WhenDelete()
    {
        var user = CreateUser();

        user.Delete();

        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserDeletedEvent>();
    }

    #region Helpers

    private static User CreateUser() =>
        new Faker<User>()
            .CustomInstantiator(faker => User.Create(
                faker.Person.UserName,
                faker.Person.Email,
                faker.PickRandom<UserRole>(),
                faker.Person.FullName,
                DateTime.UtcNow.AddYears(-30),
                "Developer",
                "password-hash"))
            .Generate();

    private static Email CreateEmail() =>
        new Faker<Email>()
            .CustomInstantiator(faker => Email.Create(faker.Person.Email))
            .Generate();

    private static UserProfile CreateUserProfile() =>
        new Faker<UserProfile>()
            .CustomInstantiator(faker => UserProfile.Create(
                faker.Person.FullName,
                DateTime.UtcNow.AddYears(-30),
                "Architect"))
            .Generate();

    #endregion
}
