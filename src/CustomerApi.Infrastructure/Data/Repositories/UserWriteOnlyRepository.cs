using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Domain.Entities.UserAggregate;
using CustomerApi.Domain.ValueObjects;
using CustomerApi.Infrastructure.Data.Context;
using CustomerApi.Infrastructure.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Repositories;

internal class UserWriteOnlyRepository(WriteDbContext dbContext)
    : BaseWriteOnlyRepository<User, Guid>(dbContext), IUserWriteOnlyRepository
{
    private static readonly Func<WriteDbContext, string, Task<bool>> ExistsByEmailCompiledAsync =
    EF.CompileAsyncQuery((WriteDbContext dbContext, string email) =>
        dbContext
            .Users
            .AsNoTracking()
            .Any(user => user.Email!.Address == email));

    private static readonly Func<WriteDbContext, string, Guid, Task<bool>> ExistsByEmailAndIdCompiledAsync =
        EF.CompileAsyncQuery((WriteDbContext dbContext, string email, Guid currentId) =>
            dbContext
              .Users
              .AsNoTracking()
              .Any(user => user.Email!.Address == email && user.Id != currentId));

    private static readonly Func<WriteDbContext, string, Task<User?>> GetByEmailCompiledAsync =
       EF.CompileAsyncQuery((WriteDbContext dbContext, string email) =>
           dbContext
               .Users
               .AsNoTracking()
               .FirstOrDefault(user => user.Email!.Address == email));
    public Task<bool> ExistsByEmailAsync(Email email) =>
        ExistsByEmailCompiledAsync(DbContext, email.Address);

    public Task<bool> ExistsByEmailAndIdAsync(Email email, Guid currentId) =>
        ExistsByEmailAndIdCompiledAsync(DbContext, email.Address, currentId);

    public Task<User?> GetByEmailAsync(Email email) =>
        GetByEmailCompiledAsync(DbContext, email.Address);

    public Task<List<User>> GetAllActivedAsync() =>
        DbContext
            .Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .ToListAsync();

    public Task<List<User>> GetAllDeactivedAsync() =>
        DbContext
            .Users
            .AsNoTracking()
            .Where(user => !user.IsActive)
            .ToListAsync();

}
