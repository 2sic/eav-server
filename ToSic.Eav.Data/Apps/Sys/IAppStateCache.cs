using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
using ToSic.Sys.Caching;
using ToSic.Sys.Caching.PiggyBack;
using ToSic.Sys.Caching.Statistics;

namespace ToSic.Eav.Apps.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppStateCache: ICacheExpiring,
    IHasMetadata,
    IHasPiggyBack,
    IAppIdentity,
    IHasMetadataSourceAndExpiring,
    IHasIdentityNameId, 
    IEntitiesSource,
    ICanBeCacheDependency,
    IHasLog,
    // IHas<IAppSpecs>,
    IRelationshipSource
{

    string? Folder { get; }

    IAppStateMetadata ThingInApp(AppThingsToStack target);

    IParentAppState ParentApp { get; }

    ICacheStatistics CacheStatistics { get; }

    int DynamicUpdatesCount { get; }

    void PreRemove();

    void DoInLock(ILog parentLog, Action transaction);

    /// <summary>
    /// Shows that the initial load has completed
    /// </summary>
    public bool FirstLoadCompleted { get; }


    /// <summary>
    /// Health-info, added in 19.03
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Health-info, added in 19.03
    /// </summary>
    string HealthMessage { get; }

    /// <summary>
    /// All ContentTypes in this App
    /// </summary>
    IEnumerable<IContentType> ContentTypes { get; }

    /// <summary>
    /// Get a content-type by name. Will also check global types if needed.
    /// </summary>
    /// <param name="name">name of the type</param>
    /// <returns>a type object or null if not found</returns>
    IContentType? TryGetContentType(string name);
}