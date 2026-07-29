using System.Collections.Concurrent;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelAnalyseUse
{
    /// <summary>
    /// The cache to reduce work.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Type> TargetTypesCache = new();

    /// <summary>
    /// Determine the type to generate for a specific model.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    /// <exception cref="TypeInitializationException"></exception>
    /// <remarks>
    /// Respects the <see cref="ModelSpecsAttribute"/> if specified.
    /// Caches the information retrieved through reflection, so much faster on the second run.
    /// </remarks>
    public static Type GetTargetType<TCustom>()
    {
        var type = typeof(TCustom);

        if (TargetTypesCache.TryGetValue(type, out var cachedType))
            return cachedType;

        var directlyAttachedTargetType = type.GetDirectGenericSubType(typeof(IModelFromEntity<>));
        // If we found a target type in the base class, cache and return it.
        if (directlyAttachedTargetType != null)
            return TargetTypesCache.GetOrAdd(type, directlyAttachedTargetType);

        // 2026-07-28 2dm - disabling attached attribute-info, but keep commented code for a while.
        //// Find attributes which describe conversion
        //var attributes = type
        //    .GetCustomAttributes(typeof(ModelSpecsAttribute), false)
        //    .Cast<ModelSpecsAttribute>()
        //    .ToList();

        //// 2025-01-21 temp
        //var implementation = attributes.FirstOrDefault()?.Use;
        //if (implementation != null)
        //    return TargetTypesCache.GetOrAdd(type, implementation);


        // Nothing found, so if it's an interface (which we can't instantiate) throw an error
        if (type.IsInterface)
            throw new TypeInitializationException(type.FullName,
                new($"Can't determine type to create of {type.Name} as it's an interface and doesn't have the proper Attributes"));
        
        // Default case: just use the same type
        TargetTypesCache.GetOrAdd(type, type);
        return type;
    }

}
