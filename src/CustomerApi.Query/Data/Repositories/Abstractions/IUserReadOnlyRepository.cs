using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerApi.Query.Abstractions;
using CustomerApi.Query.QueriesModel;

namespace CustomerApi.Query.Data.Repositories.Abstractions;

public interface IUserReadOnlyRepository : IReadOnlyRepository<UserQueryModel, Guid>
{
    Task<IEnumerable<UserQueryModel>> GetAllAsync();
}