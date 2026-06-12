using System.Collections.Generic;
using CustomerApi.Core.AppSettings;
using CustomerApi.Core.Extensions;
using CustomerApi.Core.Extensions.ServiceCollectionsExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Core;

[UnitTest]
public class ServicesCollectionExtensionsTests
{
    private const int AbsoluteExpirationInHours = 4;
    private const int SlidingExpirationInSeconds = 120;

    [Fact]
    public void Should_ReturnClassOptions_WhenConfigureAppSettings()
    {
        var serviceProvider = CreateServiceProvider();

        var act = serviceProvider.GetOptions<CacheOptions>();

        act.Should().NotBeNull();
        act.AbsoluteExpirationInHours.Should().Be(AbsoluteExpirationInHours);
        act.SlidingExpirationInSeconds.Should().Be(SlidingExpirationInSeconds);
    }

    #region Helpers

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(_ => CreateConfiguration());
        services.ConfigureAppSettings();

        return services.BuildServiceProvider(true);
    }

    private static IConfiguration CreateConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "CacheOptions:AbsoluteExpirationInHours", AbsoluteExpirationInHours.ToString() },
            { "CacheOptions:SlidingExpirationInSeconds", SlidingExpirationInSeconds.ToString() }
        });

        return configurationBuilder.Build();
    }

    #endregion
}
