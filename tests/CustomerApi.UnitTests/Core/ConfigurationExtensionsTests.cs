using System.Collections.Generic;
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
    private const int AbsoluteExpirationInHours = 4;
    private const int SlidingExpirationInSeconds = 120;

    [Fact]
    public void Should_ReturnsClassOptions_WhenGetOptions()
    {
        // Prepara o cenario.
        var configuration = CreateConfiguration();

        // Executa a acao.
        var act = configuration.GetOptions<CacheOptions>();

        // Valida o resultado.
        act.Should().NotBeNull();
        act.AbsoluteExpirationInHours.Should().Be(AbsoluteExpirationInHours);
        act.SlidingExpirationInSeconds.Should().Be(SlidingExpirationInSeconds);
    }

    #region Helpers

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
