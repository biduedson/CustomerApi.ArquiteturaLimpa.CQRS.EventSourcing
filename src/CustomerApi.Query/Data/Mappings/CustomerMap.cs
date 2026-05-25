using CustomerApi.Query.Abstractions;
using CustomerApi.Query.QueriesModel;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CustomerApi.Query.Data.Mappings;

public class CustomerMap : IReadDbMapping
{
    public void Configure()
    {
        BsonClassMap.TryRegisterClassMap<CustomerQueryModel>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapMember(customer => customer.Id)
               .SetIsRequired(true);

            classMap.MapMember(customer => customer.FirstName)
               .SetIsRequired(true);

            classMap.MapMember(customer => customer.LastName)
               .SetIsRequired(true);

            classMap.MapMember(customer => customer.Gender)
            .SetIsRequired(true);

            classMap.MapMember(customer => customer.Email)
              .SetIsRequired(true);

            classMap.MapMember(customer => customer.DateOfBirth)
              .SetIsRequired(true)
              .SetSerializer(new DateTimeSerializer(true));

            classMap.UnmapMember(customer => customer.FullName);
        });
    }
}
