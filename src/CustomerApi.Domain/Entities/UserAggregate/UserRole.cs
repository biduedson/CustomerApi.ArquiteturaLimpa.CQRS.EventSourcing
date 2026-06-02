using System.Text.Json.Serialization;

namespace CustomerApi.Domain.Entities.UserAggregate;

[JsonConverter(typeof(JsonNumberEnumConverter<UserRole>))]
public enum UserRole
{
    Viewer = 1,

    Operator = 2,

    Admin = 3
}