using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Infrastructure.Extensions.EntityTypeBuilderExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerApi.Infrastructure.Data.Mappings;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder
            .ConfigureBaseEntity();

        builder
            .Property(customer => customer.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(customer => customer.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(customer => customer.Gender)
            .IsRequired()
            .HasMaxLength(6)
            .HasConversion<string>();

        builder.OwnsOne(customer => customer.Email, ownedNav =>
        {
            ownedNav
                .Property(email => email.Address)
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnName(nameof(Customer.Email));

            ownedNav
                .HasIndex(email => email.Address)
                .IsUnique();
        });

        builder
            .Property(customer => customer.DateOfBirth)
            .IsRequired()
            .HasColumnType("DATE");
    }
}
