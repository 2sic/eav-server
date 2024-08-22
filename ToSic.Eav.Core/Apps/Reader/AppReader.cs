﻿using System.Collections.Immutable;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using ToSic.Sxc.Apps;

namespace ToSic.Eav.Apps.Services;

public class AppReader() : ServiceBase("App.Reader"), IAppReader, IAppSpecsWithStateAndCache, IMetadataSource, IHasMetadataSource
{
    internal AppReader Init(IAppStateCache appState, ILog parentLog)
    {
        _appState = appState as AppState;
        this.LinkLog(parentLog);
        return this;
    }
    private AppState _appState;

    /// <inheritdoc />
    public IAppSpecs Specs => _specs ??= new AppSpecsForAppStateInCache(_appState);
    private IAppSpecs _specs;

    #region Identity

    public int ZoneId => _appState.ZoneId;

    public int AppId => _appState.AppId;

    #endregion

    #region Basic Properties

    public string Name => _appState.Name;

    public string Folder => _appState.Folder;

    public string NameId => _appState.NameId;

    #endregion

    #region Advanced Properties

    public IAppConfiguration Configuration => _appConfig.Get(() => new AppConfiguration(ConfigurationEntity));
    private readonly GetOnce<IAppConfiguration> _appConfig = new();

    public IEntity ConfigurationEntity => _appConfiguration ??= _appState.SettingsInApp.AppConfiguration;

    private IEntity _appConfiguration;

    #endregion

    #region PiggyBack

    PiggyBack IHasPiggyBack.PiggyBack => _appState.PiggyBack;

    #endregion

    #region Internal


    IAppStateCache IAppReader.StateCache => _appState;
    IAppStateCache IAppReader.ParentAppState => _appState.ParentApp?.AppState;
    //SynchronizedEntityList IAppReader.ListCache => _appState.ListCache;

    SynchronizedList<IEntity> IAppReader.ListPublished => _appState.ListPublished;

    SynchronizedList<IEntity> IAppReader.ListNotHavingDrafts => _appState.ListNotHavingDrafts;
    AppStateMetadata IAppReader.SettingsInApp => _appState.SettingsInApp;

    AppStateMetadata IAppReader.ResourcesInApp => _appState.ResourcesInApp;

    ParentAppState IAppReader.ParentApp => _appState.ParentApp;

    AppRelationshipManager IAppReader.Relationships => _appState.Relationships;

    #endregion




    public IImmutableList<IEntity> List => _appState.List;
    public IEntity GetDraft(IEntity entity) => _appState.GetDraft(entity);

    public IEntity GetPublished(IEntity entity) => _appState.GetPublished(entity);


    public IEnumerable<IContentType> ContentTypes => _appState.ContentTypes;

    public IContentType GetContentType(string name) => _appState.GetContentType(name);

    public IContentType GetContentType(int contentTypeId) => _appState.GetContentType(contentTypeId);

    public IMetadataOf Metadata => _appState.Metadata;

    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
        => _appState.GetMetadata(targetType, key, contentTypeName);

    public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null) 
        => _appState.GetMetadata(targetType, key, contentTypeName);


    #region Timestamps

    public long CacheTimestamp => _appState.CacheTimestamp;

    public bool CacheChanged(long dependentTimeStamp) => _appState.CacheChanged(dependentTimeStamp);

    #endregion


    IMetadataOf IMetadataOfSource.GetMetadataOf<T>(TargetTypes targetType, T key, string title) => _appState.GetMetadataOf(targetType, key, title);

    IAppSpecs IHas<IAppSpecs>.Value => this;
    IAppSpecsWithState IHas<IAppSpecsWithState>.Value => this;

    IAppStateCache IAppSpecsWithStateAndCache.Cache => _appState;

    IAppSpecsWithStateAndCache IHas<IAppSpecsWithStateAndCache>.Value => this;
    public IMetadataSource MetadataSource => ((IHasMetadataSource)_appState).MetadataSource;
}