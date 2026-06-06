using CustomerApi.Domain.Entities.UserSessionAggregate;
using CustomerApi.Infrastructure.Extensions.EntityTypeBuilderExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerApi.Infrastructure.Data.Mappings;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ConfigureBaseEntity();

        builder
            .Property(session => session.UserId)
            .IsRequired();

        builder
            .Property(session => session.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder
            .HasIndex(session => session.RefreshTokenHash)
            .IsUnique();

        builder
            .Property(session => session.UserAgent)
            .IsRequired()
            .HasMaxLength(512);

        builder
            .Property(session => session.IpAddress)
            .HasMaxLength(64);

        builder
            .Property(session => session.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder
            .Property(session => session.RevocationReason)
            .HasMaxLength(200);

        builder
            .Property(session => session.CreatedAt)
            .IsRequired()
            .HasColumnType("DATETIME2");

        builder
            .Property(session => session.ExpiresAt)
            .IsRequired()
            .HasColumnType("DATETIME2");

        builder
            .Property(session => session.LastUsedAt)
            .HasColumnType("DATETIME2");

        builder
            .Property(session => session.RevokedAt)
            .HasColumnType("DATETIME2");
    }
}
