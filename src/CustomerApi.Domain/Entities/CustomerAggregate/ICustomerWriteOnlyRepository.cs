using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.ValueObjects;

namespace CustomerApi.Domain.Entities.CustomerAggregate;

public interface ICustomerWriteOnlyRepository : IWriteOnlyRepository<Customer,Guid>
{
    Task<bool> ExistsByEmailAsync(Email email);

    Task<bool> ExistsByEmailAsync(Email email, Guid currentId);
}
