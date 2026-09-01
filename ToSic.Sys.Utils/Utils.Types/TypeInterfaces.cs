using System.Collections.Concurrent;
using ToSic.Sys.Performance;

namespace ToSic.Sys.Utils.Types;

/// <summary>
/// Special helper to ....
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static partial class TypeInterfaces
{
    /// <summary>
    /// A thread-safe cache to store our compiled delegates.
    /// These are important for fast creation without ongoing reflection.
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, IList<Type>> DirectInterfacesCache = new();

    /// <summary>
    /// Get interfaces directly attached to a type, excluding those inherited from base classes or other interfaces.
    /// </summary>
    /// <param name="type"></param>
    /// <remarks>
    /// WARNING: It's easy to get this wrong, because re-implementing an interface will usually not work as expected,
    /// because interfaces are flattened at compile time. So using this method will never catch re-implemented interfaces.
    /// </remarks>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IList<Type> GetDirectInterfaces(this Type type)
        => type != null
            ? DirectInterfacesCache.GetOrAdd(type, GetDirectInterfacesInternal)
            : throw new ArgumentNullException(nameof(type));

    public static Type? GetDirectGenericSubType(this Type type, Type genericType, int argumentIndex = 0)
    {
        var genericInterfaceType = type.GetDirectInterfaces()
            .FirstOrDefault(i => i.IsGenericTypeOf(genericType));

        return genericInterfaceType?.GetGenericArguments()[argumentIndex];
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsGenericTypeOf(this Type type, Type genericType)
        => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

    private static IList<Type> GetDirectInterfacesInternal(Type type)
    {
        // 1. Get interfaces inherited from base classes (or empty list, of no base class)
        var baseInterfaces = type.BaseType?.GetInterfaces() ?? Type.EmptyTypes;

        // 2. Filter out base class interfaces
        var declaredInterfaces = type.GetInterfaces()
            .Except(baseInterfaces)
            .ToList();

        // 3. Filter out interfaces that are inherited by other interfaces in the declared list
        var transitiveInterfaces = declaredInterfaces.SelectMany(i => i.GetInterfaces());

        return declaredInterfaces.Except(transitiveInterfaces).ToListOpt();
    }

    public static Type? GetGenericSubType(this Type type, Type genericType, int argumentIndex = 0)
    {
        var genericInterfaceType = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericTypeOf(genericType));

        return genericInterfaceType?.GetGenericArguments()[argumentIndex];
    }

}