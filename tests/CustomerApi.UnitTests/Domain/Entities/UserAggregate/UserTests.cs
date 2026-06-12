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
        // Prepara o cenario.
        // Executa a acao.
        var act = CreateUser();

        // Valida o resultado.
        act.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserCreatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeEmail()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.ChangeEmail(CreateEmail());

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeProfile()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.ChangeProfile(CreateUserProfile());

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenChangeRole()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.ChangeRole(UserRole.Admin);

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserPasswordChangedEvent_WhenChangePassword()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.ChangePassword("new-password-hash");

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserPasswordChangedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenDeactivate()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.Deactivate();

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserUpdatedEvent_WhenActivate()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.Deactivate();
        user.Activate();

        // Valida o resultado.
        user.DomainEvents.Should()
            .NotBeNullOrEmpty()
            .And.OnlyHaveUniqueItems()
            .And.ContainItemsAssignableTo<UserUpdatedEvent>();
    }

    [Fact]
    public void Should_UserDeletedEvent_WhenDelete()
    {
        // Prepara o cenario.
        var user = CreateUser();

        // Executa a acao.
        user.Delete();

        // Valida o resultado.
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
