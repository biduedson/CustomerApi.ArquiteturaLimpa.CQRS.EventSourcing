
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Core;

[UnitTest]
public class ConfigurationExtensionsTests
{
    [Fact]
    public void Should_ReturnsClassOptions_WhenGetOptions()
    {
        const int absoluteExpirationInHours = 4;
        const int slidingExpirationInSeconds = 120;

        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheOptions:AbsoluteExpirationInHours", absoluteExpirationInHours.ToString() },
            { "CacheOptions:SlidingExpirationInSeconds", slidingExpirationInSeconds.ToString() }
        });

        var configuration = configurationBuilder.Build();

        var act = configuration.GetOptions<CacheOptions>();

        act.Should().NotBeNull();
        act.AbsoluteExpirationInHours.Should().Be(absoluteExpirationInHours);
        act.SlidingExpirationInSeconds.Should().Be(slidingExpirationInSeconds);
    }
}
