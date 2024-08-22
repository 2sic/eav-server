using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

/// <summary>
/// This is the internal, official way to access data from an App.
/// * In certain cases it will do other things, such as retrieve more data from elsewhere to be sure that everything is available.
/// * It is for short term use only, so don't cache this object
///
/// To get an app Reader, use the ??? TODO
/// </summary>
public interface IAppReader: IAppState, IHasPiggyBack, IMetadataOfSource, IHas<IAppSpecs>, IAppSpecsWithStateAndCache, IHasMetadataSource
{
    IAppSpecs Specs { get; }

    IAppStateCache StateCache { get; }

    IAppStateCache ParentAppState { get; }

    public SynchronizedList<IEntity> ListPublished { get; }

    SynchronizedList<IEntity> ListNotHavingDrafts { get; }


    AppStateMetadata SettingsInApp { get; }

    AppStateMetadata ResourcesInApp { get; }

    IEntity ConfigurationEntity { get; }

    ParentAppState ParentApp { get; }

    AppRelationshipManager Relationships { get; }
}