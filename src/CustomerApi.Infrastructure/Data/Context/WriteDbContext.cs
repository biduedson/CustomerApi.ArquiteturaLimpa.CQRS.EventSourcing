using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Context;

public class WriteDbContext(DbContextOptions<WriteDbContext> dbOptions)
    : BaseDbContext<WriteDbContext>(dbOptions)
{
    public  DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
