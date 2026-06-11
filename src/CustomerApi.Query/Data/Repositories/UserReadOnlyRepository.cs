using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.Data.Repositories.Abstractions;
using CustomerApi.Query.QueriesModel;
using MongoDB.Driver;

namespace CustomerApi.Query.Data.Repositories;

public class UserReadOnlyRepository(IReadDbContext readDbContext)
: BaseReadOnlyRepository<UserQueryModel, Guid>(readDbContext), IUserReadOnlyRepository
{
    public async Task<IEnumerable<UserQueryModel>> GetAllAsync()
    {
        var sort = Builders<UserQueryModel>.Sort
        .Ascending(user => user.UserName)
        .Descending(user => user.DateOfBirth);

        var findOptions = new FindOptions<UserQueryModel>
        {
            Sort = sort
        };

        using var asyncCursor = await Collection.FindAsync(Builders<UserQueryModel>.Filter.Empty, findOptions);
        return await asyncCursor.ToListAsync();
    }
}