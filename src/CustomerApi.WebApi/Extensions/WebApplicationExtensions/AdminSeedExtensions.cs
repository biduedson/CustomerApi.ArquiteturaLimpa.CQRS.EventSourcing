using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerApi.Application.Abstractions.Auth;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerApi.WebApi.Extensions.WebApplicationExtensions;

public static class AdminSeedExtensions
{
    internal static async Task SeedAdminUserAsync(this WebApplication app)
    {
        await using var serviceScope = app.Services.CreateAsyncScope();

        var options = serviceScope.ServiceProvider.GetOptions<AdminSeedOptions>();

        if (options is not { Enabled: true })
            return;

        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<AdminSeedOptions>>();
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserWriteOnlyRepository>();
        var passwordHasher = serviceScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var validationErrors = Validate(options);
        if (validationErrors.Count > 0)
        {
            logger.LogWarning(
                "----- AdminSeed: configuracoes invalidas: {Errors}",
                string.Join("; ", validationErrors));

            return;
        }

        var role = Enum.Parse<UserRole>(options.Role, ignoreCase: true);
        var email = Email.Create(options.Email);

        if (await userRepository.ExistsByEmailAsync(email))
        {
            logger.LogInformation("----- AdminSeed: usuario com email configurado ja existe.");
            return;
        }

        if (await userRepository.ExistsByUserNameAsync(options.Username))
        {
            logger.LogInformation("----- AdminSeed: usuario com username configurado ja existe.");
            return;
        }

        var password = Password.Create(options.Password);
        var passwordHash = passwordHasher.Hash(password.ToString()!);

        var user = User.Create(
            options.Username,
            email.Address,
            role,
            options.FullName,
            options.DateOfBirth!.Value,
            options.JobTitle,
            passwordHash);

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("----- AdminSeed: usuario administrador inicial criado com sucesso.");
    }

    private static List<string> Validate(AdminSeedOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Username))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:Username e obrigatorio");

        if (string.IsNullOrWhiteSpace(options.Email))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:Email e obrigatorio");

        if (!Enum.TryParse<UserRole>(options.Role, ignoreCase: true, out var role))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:Role invalida");

        if (role != UserRole.Admin)
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:Role deve ser Admin");

        if (string.IsNullOrWhiteSpace(options.FullName))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:FullName e obrigatorio");

        if (options.DateOfBirth is null || options.DateOfBirth == default)
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:DateOfBirth e obrigatorio");

        if (string.IsNullOrWhiteSpace(options.JobTitle))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:JobTitle e obrigatorio");

        if (string.IsNullOrWhiteSpace(options.Password))
            errors.Add($"{AdminSeedOptions.ConfigSectionPath}:Password e obrigatorio");

        return errors;
    }
}
