using CustomerApi.Query.Abstractions;
using CustomerApi.Query.QueriesModel;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CustomerApi.Query.Data.Mappings;

public class UserMap : IReadDbMapping
{
    public void Configure()
    {
        BsonClassMap.TryRegisterClassMap<UserQueryModel>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapMember(user => user.Id)
               .SetIsRequired(true);

            classMap.MapMember(user => user.UserName)
               .SetIsRequired(true);

            classMap.MapMember(user => user.Email)
               .SetIsRequired(true);

            classMap.MapMember(user => user.Role)
            .SetIsRequired(true);

            classMap.MapMember(user => user.FullName)
                       .SetIsRequired(true);

            classMap.MapMember(user => user.DateOfBirth)
             .SetIsRequired(true)
             .SetSerializer(new DateTimeSerializer(true));

            classMap.MapMember(user => user.JobTitle)
                      .SetIsRequired(true);

            classMap.MapMember(user => user.IsActive)
            .SetIsRequired(true);

        });
    }
}