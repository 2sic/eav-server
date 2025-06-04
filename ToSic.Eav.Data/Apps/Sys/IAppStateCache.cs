using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Metadata;
using ToSic.Lib.Data;
using ToSic.Sys.Caching;
using ToSic.Sys.Caching.PiggyBack;
using ToSic.Sys.Caching.Statistics;

namespace ToSic.Eav.Apps.State;

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

    string Folder { get; }

    IAppStateMetadata ThingInApp(AppThingsToStack target);

    IParentAppState ParentApp { get; }

    ICacheStatistics CacheStatistics { get; }

    int DynamicUpdatesCount { get; }

    void PreRemove();

    void DoInLock(ILog parentLog, Action transaction);

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
    IContentType GetContentType(string name);
}