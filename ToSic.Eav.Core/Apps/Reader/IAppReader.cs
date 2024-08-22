using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;
using ToSic.Lib.Data;

namespace ToSic.Eav.Apps;

/// <summary>
/// This is the internal, official way to access data from an App.
/// * In certain cases it will do other things, such as retrieve more data from elsewhere to be sure that everything is available.
/// * It is for short term use only, so don't cache this object
///
/// To get an app Reader, use the ??? TODO
/// </summary>
public interface IAppReader:
    IAppIdentity,
    IAppReadEntities,
    IAppReadContentTypes,
    IHasPiggyBack,
    IMetadataOfSource
{
    IAppSpecs Specs { get; }

    IAppReadEntities Entities { get; }

    IAppReadContentTypes ContentTypesSvc { get; }

    IAppStateCache StateCache { get; }

    IAppStateCache ParentAppState { get; }

    public SynchronizedList<IEntity> ListPublished { get; }

    SynchronizedList<IEntity> ListNotHavingDrafts { get; }

    //ParentAppState ParentApp { get; }

    AppRelationshipManager Relationships { get; }

    IMetadataSource Metadata { get; }
}