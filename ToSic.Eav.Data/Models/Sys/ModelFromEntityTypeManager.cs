using System.Collections.Concurrent;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelFromEntityTypeManager
{
    /// <summary>
    /// Determine the type to generate for a specific model or interface.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    /// <exception cref="TypeInitializationException"></exception>
    /// <remarks>
    /// Respects the <see cref="ModelSpecsAttribute"/> if specified.
    /// Caches the information retrieved through reflection, so much faster on the second run.
    /// </remarks>
    public static Type GetTargetType<TModel>()
        where TModel : class, IModelFromData
        => TargetTypesCache.GetOrAdd(typeof(TModel), FindTargetType);

    /// <summary>
    /// The cache to reduce work.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Type> TargetTypesCache = new();

    /// <summary>
    /// Get the generic subtype as specified by the `IModelFromEntity` - if provided.
    /// </summary>
    /// <param name="type"></param>
    /// <remarks>
    /// For class models, only respect the directly attached interface definitions, for interfaces, also accept inherited definitions, because:
    /// 1. The interface must implement <see cref="IModelFromEntity{TConcreteModel}"/> - in which case the `TConcreteModel` is the target type.
    ///    So if we ask the interface, we should get this information and the target type.
    /// 2. The class must implement the interface, so it will indirectly inherit the same interface, which only works in basic scenarios.
    /// In advanced scenarios where a class _inherits_ from another class, this will fail.
    ///
    /// Here's why classes should only use the directly attached interface, and not the inherited one:
    /// 1. The base class (`ModelBase`) implements the interface incl. the `IModelFromEntity{ModelBase}`, pretending to only be a model of itself.
    /// 2. An inheriting class will also inherit this interface which would result in a `ModelBase` if we also accepted inherited interfaces.
    /// 3. This would later cause a null-cast because the base class cannot be cast as the inheriting class.
    ///
    /// Here's why the interface should go deeper than just the directly attached base class:
    /// </remarks>
    /// <returns></returns>
    /// <exception cref="TypeInitializationException"></exception>
    private static Type FindTargetType(Type type)
    {
        var result = type.IsInterface
            // For interfaces, check all implemented interfaces, incl. derived ones
            ? type.GetGenericSubType(typeof(IModelFromEntity<>))
              // Nothing found. Since we can't instantiate an interface, throw
              ?? throw new TypeInitializationException(
                  type.FullName,
                  new($"Can't determine type to create of {type.Name}. " +
                      $"The interface doesn't have the proper `{nameof(IModelFromEntity<>)}`.")
              )
            // For classes, only check the directly attached interface - as it could specify a derived type with more properties.
            // But never check inherited ones.
            // Then fallback to itself as that would be the implementation
            : type.GetDirectGenericSubType(typeof(IModelFromEntity<>))
              ?? type;
        
        return result;
    }

}
