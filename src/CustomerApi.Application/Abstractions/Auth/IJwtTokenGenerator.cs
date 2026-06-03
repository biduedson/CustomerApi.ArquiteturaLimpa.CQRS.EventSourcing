using CustomerApi.Domain.Entities.UserAggregate;

namespace CustomerApi.Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
}