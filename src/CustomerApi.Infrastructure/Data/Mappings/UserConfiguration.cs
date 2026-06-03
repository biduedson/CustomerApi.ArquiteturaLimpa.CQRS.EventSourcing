using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerApi.Infrastructure.Data.Mappings;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .ConfigureBaseEntity();

        builder
            .Property(user => user.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(user => user.Email, ownedNav =>
        {
            ownedNav
                .Property(email => email.Address)
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnName(nameof(User.Email));

            ownedNav
                .HasIndex(email => email.Address)
                .IsUnique();
        });

        builder
            .Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.OwnsOne(user => user.Profile, ownedNav =>
        {
            ownedNav
                .Property(profile => profile.FullName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName(nameof(User.Profile) + "_" + nameof(UserProfile.FullName));

            ownedNav
                .Property(profile => profile.DateOfBirth)
                .IsRequired()
                .HasColumnType("DATE")
                .HasColumnName(nameof(User.Profile) + "_" + nameof(UserProfile.DateOfBirth));

            ownedNav
                .Property(profile => profile.JobTitle)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName(nameof(User.Profile) + "_" + nameof(UserProfile.JobTitle));
        });

        builder
            .Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder
            .Property(user => user.IsActive)
            .IsRequired();

        builder
             .Property(user => user.CreatedAt)
             .IsRequired()
             .HasColumnType("DATETIME2");

        builder
            .Property(user => user.UpdatedAt)
            .HasColumnType("DATETIME2");
    }
}
