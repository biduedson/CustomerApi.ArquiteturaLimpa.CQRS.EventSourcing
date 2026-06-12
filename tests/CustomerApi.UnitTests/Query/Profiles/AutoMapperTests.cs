

using System;
using AutoMapper;
using CustomerApi.Query.Profiles;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Categories;

namespace CustomerApi.UnitTests.Query.Profiles;

[UnitTest]
public class AutoMapperTests
{
    [Fact]
    public void Should_Mapper_ConfigurationIsValid()
    {
        // Prepara o cenario.

        var config = new MapperConfiguration(cfg => cfg.AddProfile<EventToQueryModelProfile>(), new NullLoggerFactory());
        var mapper = new Mapper(config);


        // Executa a acao.
        var act = new Action(() => mapper.ConfigurationProvider.AssertConfigurationIsValid());

        // Valida o resultado.
        act.Should().NotThrow();
    }
}
