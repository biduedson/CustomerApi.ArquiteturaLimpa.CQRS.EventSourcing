using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.ValueObjects;

namespace CustomerApi.Domain.Entities.CustomerAggregate;

public interface ICustomerWriteOnlyRepository : IWriteOnlyRepository<Customer,Guid>
{
    Task<bool> ExistsByEmailAsyn(Email email);

    Task<bool> ExistsByEmailAsyn(Email email, Guid currentId);
}
