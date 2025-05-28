using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Metadata;
using ToSic.Lib.Caching;
using ToSic.Lib.Caching.PiggyBack;
using ToSic.Lib.Caching.Statistics;
using ToSic.Lib.Data;

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

    AppStateMetadata ThingInApp(AppThingsToStack target);

    ParentAppState? ParentApp { get; }

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