using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

public interface IAppReader: IAppState, IHasPiggyBack, IMetadataOfSource, IHas<IAppSpecs>, IAppSpecsWithStateAndCache, IHasMetadataSource
{
    IAppStateCache StateCache { get; }

    IAppStateCache ParentAppState { get; }

    //SynchronizedEntityList ListCache { get; }

    public SynchronizedList<IEntity> ListPublished { get; }

    SynchronizedList<IEntity> ListNotHavingDrafts { get; }


    AppStateMetadata SettingsInApp { get; }

    AppStateMetadata ResourcesInApp { get; }

    IEntity ConfigurationEntity { get; }

    ParentAppState ParentApp { get; }

    AppRelationshipManager Relationships { get; }
}