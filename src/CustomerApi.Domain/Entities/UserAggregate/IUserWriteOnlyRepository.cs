using CustomerApi.Core.SharedKernel;
using CustomerApi.Domain.ValueObjects;

namespace CustomerApi.Domain.Entities.UserAggregate;

public interface IUserWriteOnlyRepository : IWriteOnlyRepository<User, Guid>
{
    Task<bool> ExistsByEmailAsync(Email email);
    Task<bool> ExistsByEmailAndIdAsync(Email email, Guid id);
    Task<User?> GetByEmailAsync(Email email);
    Task<List<User>> GetAllDeactivedAsync();
}