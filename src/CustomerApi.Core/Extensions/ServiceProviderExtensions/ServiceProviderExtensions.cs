using CustomerApi.Core.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomerApi.Core.Extensions;

public static class ServiceProviderExtensions
{
    public static TOptions GetOptions<TOptions>(this IServiceProvider serviceProvider)
        where TOptions : class, IAppOptions =>
        serviceProvider.GetService<IOptions<TOptions>>()?.Value!;
}
