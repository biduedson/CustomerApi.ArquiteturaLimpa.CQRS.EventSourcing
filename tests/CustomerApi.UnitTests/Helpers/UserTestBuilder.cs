using Bogus;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Infrastructure.Auth.Password;

namespace CustomerApi.UnitTests.Helpers;

internal static class UserTestBuilder
{
    public static User Create(string password, bool inactive = false)
    {
        var passwordHasher = new BCryptPasswordHasher();

        var user = new Faker<User>()
            .CustomInstantiator(faker => User.Create(
                faker.Person.UserName,
                faker.Person.Email,
                faker.PickRandom<UserRole>(),
                faker.Person.FullName,
                faker.Person.DateOfBirth,
                "Gerente",
                passwordHasher.Hash(password)))
            .Generate();

        if (inactive)
            user.Deactivate();

        return user;
    }
}
