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
}