using CustomerApi.Core.SharedKernel;
using CustomerApi.Infrastructure.Data;
using CustomerApi.Infrastructure.Data.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CustomerApi.UnitTests.Helpers;

internal static class TestUnitOfWorkFactory
{
    public static UnitOfWork Create(WriteDbContext context) => new(
        context,
        Substitute.For<IEventStoreRepository>(),
        Substitute.For<IMediator>(),
        Substitute.For<ILogger<UnitOfWork>>());
}
