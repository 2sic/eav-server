using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Metadata;
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

    ParentAppState ParentApp { get; }

    ICacheStatistics CacheStatistics { get; }

    int DynamicUpdatesCount { get; }

    void PreRemove();

    void DoInLock(ILog parentLog, Action transaction);
}