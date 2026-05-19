using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Domain.Entities.CustomerAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Infrastructure.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Repositories;

internal class CustomerWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<Customer,Guid>(dbContext), ICustomerWriteOnlyRepository
{
    private static readonly Func<WriteDbContext, string, Task<bool>> ExistsByEmailCompiledAsync =
        EF.CompileAsyncQuery((WriteDbContext dbContext, string email) =>
        dbContext
        .Customers
        .AsNoTracking()
        .Any(customer => customer.Email!.Address == email));

    private static readonly Func<WriteDbContext, string, Guid, Task<bool>> ExistsByEmailAndIdCompiledAsync =
       EF.CompileAsyncQuery((WriteDbContext dbContext, string email, Guid currentId) =>
           dbContext
               .Customers
               .AsNoTracking()
               .Any(customer => customer.Email!.Address == email && customer.Id != currentId));

    public Task<bool> ExistsByEmailAsync(Email email) =>
        ExistsByEmailCompiledAsync(DbContext, email.Address);

    public Task<bool> ExistsByEmailAsync(Email email, Guid currentId) =>
        ExistsByEmailAndIdCompiledAsync(DbContext, email.Address, currentId);
}
