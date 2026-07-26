using System.Collections.Concurrent;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Special system to manage and to convert c# classes with their definitions/attributes into content types.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypesFromCodeManager(LazySvc<ContentTypesFromCodeBuilder> ctBuilder)
    : ServiceBase("Eav.CtFact")
{
    [PrivateApi("TODO: Should probably be something different...?")]
    public const int NoAppId = -1;

    /// <summary>
    /// Get the ContentType for a given class. If it doesn't exist yet, it will be created and cached.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IContentType Get<T>()
        => Get(typeof(T));

    /// <summary>
    /// Get the ContentType for a given class. If it doesn't exist yet, it will be created and cached.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IContentType Get(Type type)
        => CtCache.GetOrAdd(type, CreateAndAddToCache);

    /// <summary>
    /// Pre-flight check if this type has configuration or not.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
#pragma warning disable CA1822
    public bool IsConfigured(Type type)
#pragma warning restore CA1822
    {
        if (IsConfiguredCache.TryGetValue(type, out var ct))
            return ct;

        // Check if we have type redirection
        var underlyingType = FindReassignedType(type);

        // Check if this is configured
        var isConfig = underlyingType.GetDirectlyAttachedAttribute<ContentTypeAttribute>() != null;

        // Remember for future use
        IsConfiguredCache[type] = isConfig;

        // if we had redirection, remember that too
        if (underlyingType != type)
            IsConfiguredCache[underlyingType] = isConfig;

        return isConfig;
    }

    private IContentType CreateAndAddToCache(Type type)
    {
        // Check if we have type redirection
        var underlyingType = FindReassignedType(type);

        // Generate
        var created = ctBuilder.Value.Generate(underlyingType,  name: null, nameId: null, scope: null);
        var isConfigured = created.RepositoryType == RepositoryTypes.CodeConfiguration;

        // Remember for future use
        CtCache[type] = created;
        IsConfiguredCache[type] = isConfigured;
        
        // if we had redirection, remember that too
        if (underlyingType != type)
        {
            CtCache[underlyingType] = created;
            IsConfiguredCache[underlyingType] = isConfigured;
        }

        return created;
    }

    private Type FindReassignedType(Type type) =>
        type.GetDirectlyAttachedAttribute<ContentTypeAssignAttribute>()?.Type ?? type;

    private static readonly ConcurrentDictionary<Type, IContentType> CtCache = new();

    private static readonly ConcurrentDictionary<Type, bool> IsConfiguredCache = new();

}