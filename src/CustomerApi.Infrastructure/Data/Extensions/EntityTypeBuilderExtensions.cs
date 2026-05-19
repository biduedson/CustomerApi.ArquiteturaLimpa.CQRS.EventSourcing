using CustomerApi.Core.SharedKernel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerApi.Infrastructure.Data.Extensions;

internal static class EntityTypeBuilderExtensions
{
    internal static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder
            .HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Ignore(entity => entity.DomainEvents);
    }
}
