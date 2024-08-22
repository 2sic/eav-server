using System.Collections.Immutable;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Reader;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps;

public class AppReader() : ServiceBase("App.Reader"), IAppReader
{
    internal AppReader Init(IAppStateCache appState, ILog parentLog)
    {
        _appState = appState as AppState;
        this.LinkLog(parentLog);
        return this;
    }
    private AppState _appState;

    /// <inheritdoc />
    public IAppSpecs Specs => _specs ??= new AppSpecs(_appState);
    private IAppSpecs _specs;

    #region Identity

    public int ZoneId => _appState.ZoneId;

    public int AppId => _appState.AppId;

    #endregion

    
    #region PiggyBack

    PiggyBack IHasPiggyBack.PiggyBack => _appState.PiggyBack;

    #endregion

    #region Normal Data / Entities / List / Draft / Publish

    public IAppReadEntities Entities => _entities ??= new AppReadEntities(_appState);
    private IAppReadEntities _entities;

    public IAppReadContentTypes ContentTypesSvc => _contentTypes ??= new AppReadContentTypes(_appState);
    private IAppReadContentTypes _contentTypes;

    public IImmutableList<IEntity> List => _appState.List;

    public SynchronizedList<IEntity> ListPublished => _appState.ListPublished;

    public SynchronizedList<IEntity> ListNotHavingDrafts => _appState.ListNotHavingDrafts;

    public IEntity GetDraft(IEntity entity) => _appState.GetDraft(entity);

    public IEntity GetPublished(IEntity entity) => _appState.GetPublished(entity);

    #endregion

    #region Internal

    public IAppStateCache StateCache => _appState;

    public IAppStateCache ParentAppState => _appState.ParentApp?.AppState;

    //ParentAppState IAppReader.ParentApp => _appState.ParentApp;

    AppRelationshipManager IAppReader.Relationships => _appState.Relationships;

    #endregion






    public IEnumerable<IContentType> ContentTypes => _appState.ContentTypes;

    public IContentType GetContentType(string name) => _appState.GetContentType(name);

    public IContentType GetContentType(int contentTypeId) => _appState.GetContentType(contentTypeId);

    #region Get Metadata

    /// <summary>
    /// Get a MetadataOf object containing a list of items + target information.
    /// </summary>
    IMetadataOf IMetadataOfSource.GetMetadataOf<T>(TargetTypes targetType, T key, string title)
        => _appState.GetMetadataOf(targetType, key, title);

    public IMetadataSource Metadata => _appState.MetadataSource;

    #endregion

}