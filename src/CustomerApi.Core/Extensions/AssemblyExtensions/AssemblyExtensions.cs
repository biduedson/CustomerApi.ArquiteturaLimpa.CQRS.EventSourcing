using System.Reflection;

namespace CustomerApi.Core.Extensions;

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetAllTypesOf<TInterface>(this Assembly assembly)
    {
        var isAssignableToInterface = typeof(TInterface).IsAssignableFrom;
        return [..assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && isAssignableToInterface(type))
            ];
    }
}
