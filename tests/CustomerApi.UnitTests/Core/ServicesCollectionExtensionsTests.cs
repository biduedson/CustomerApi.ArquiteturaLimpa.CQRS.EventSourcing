using System.Collections.Generic;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions.ServiceCollectionsExtensions;
using CustomerApi.Core.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Core;

[UnitTest]
public class ServicesCollectionExtensionsTests
{
    [Fact]
    public void Should_ReturnClassOptions_WhenConfigureAppSettings()
    {
        const int absoluteExpirationInHours = 4;
        const int slidingExpirationInSeconds = 120;

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "CacheOptions:AbsoluteExpirationInHours", absoluteExpirationInHours.ToString() },
            { "CacheOptions:SlidingExpirationInSeconds", slidingExpirationInSeconds.ToString() }
        });

        var configuration = configurationBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(_ => configuration);
        services.ConfigureAppSettings();
        var serviceProvider = services.BuildServiceProvider(true);

        var act = serviceProvider.GetOptions<CacheOptions>();

        act.Should().NotBeNull();
        act.AbsoluteExpirationInHours.Should().Be(absoluteExpirationInHours);
        act.SlidingExpirationInSeconds.Should().Be(slidingExpirationInSeconds);
    }


}
