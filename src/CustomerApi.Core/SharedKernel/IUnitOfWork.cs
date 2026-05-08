
namespace CustomerApi.Core.SharedKernel;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
