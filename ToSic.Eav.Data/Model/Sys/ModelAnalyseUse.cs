using System.Collections.Concurrent;

namespace ToSic.Eav.Model.Sys;

public static class ModelAnalyseUse
{
    /// <summary>
    /// Determine the type to generate for a specific model.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    /// <exception cref="TypeInitializationException"></exception>
    /// <remarks>
    /// Respects the <see cref="ModelCreationAttribute"/> if specified.
    /// Caches the information retrieved through reflection, so much faster on the second run.
    /// </remarks>
    public static Type GetTargetType<TCustom>()
    {
        var type = typeof(TCustom);

        if (TargetTypes.TryGetValue(type, out var cachedType))
            return cachedType;

        // Find attributes which describe conversion
        var attributes = type
            .GetCustomAttributes(typeof(ModelCreationAttribute), false)
            .Cast<ModelCreationAttribute>()
            .ToList();

        // 2025-01-21 temp
        var implementation = attributes.FirstOrDefault()?.Use;
        if (implementation != null)
            return TargetTypes.GetOrAdd(type, implementation);


        if (type.IsInterface)
            throw new TypeInitializationException(type.FullName,
                new($"Can't determine type to create of {type.Name} as it's an interface and doesn't have the proper Attributes"));
        TargetTypes.GetOrAdd(type, type);
        return type;
    }

    private static readonly ConcurrentDictionary<Type, Type> TargetTypes = new();
}
