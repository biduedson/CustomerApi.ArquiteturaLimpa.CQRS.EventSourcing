using CustomerApi.Core.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerApi.Infrastructure.Data.Mappings;

internal class EventStoreConfiguration : IEntityTypeConfiguration<EventStore>
{
    public void Configure(EntityTypeBuilder<EventStore> builder)
    {
        builder
            .HasKey(eventStore => eventStore.Id);

        builder
            .Property(eventStore => eventStore.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Property(eventStore => eventStore.AggregateId)
            .IsRequired()
            .ValueGeneratedNever();

        builder
            .Property(eventStore => eventStore.MessageType)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(eventStore => eventStore.Data)
            .IsRequired()
            .HasComment("Evento serializado em JSON");

        builder
            .Property(eventStore => eventStore.OccurreedOn)
            .IsRequired()
            .HasComment("CreatedAt");
    }
}
