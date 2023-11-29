using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using ToSic.Sxc.Apps;

namespace ToSic.Eav.Apps.Reader
{
    internal class AppStateReader: HelperBase, IAppState, IAppStateInternal, IMetadataSource
    {
        private readonly AppState _appState;

        public AppStateReader(AppState appState, ILog parentLog): base(parentLog, "App.Reader")
        {
            _appState = appState;
        }

        #region Identity

        public int ZoneId => _appState.ZoneId;

        public int AppId => _appState.AppId;


        #endregion

        #region PiggyBack

        PiggyBack IHasPiggyBack.PiggyBack => _appState.PiggyBack;

        #endregion

        #region Internal

        
        AppState IAppStateInternal.StateCache => _appState;
        AppState IAppStateInternal.ParentAppState => _appState.ParentApp?.AppState;
        SynchronizedEntityList IAppStateInternal.ListCache => _appState.ListCache;

        SynchronizedList<IEntity> IAppStateInternal.ListPublished => _appState.ListPublished;

        SynchronizedList<IEntity> IAppStateInternal.ListNotHavingDrafts => _appState.ListNotHavingDrafts;
        AppStateMetadata IAppStateInternal.SettingsInApp => _appState.SettingsInApp;

        AppStateMetadata IAppStateInternal.ResourcesInApp => _appState.ResourcesInApp;

        IContentType IAppStateInternal.GetContentType(int contentTypeId) => _appState.GetContentType(contentTypeId);
        ParentAppState IAppStateInternal.ParentApp => _appState.ParentApp;

        #endregion

        #region Basic Properties

        public string Name => _appState.Name;

        public string Folder => _appState.Folder;

        public string NameId => _appState.NameId;

        #endregion

        #region Advanced Properties

        public IAppConfiguration Configuration => _appConfig.Get(() => new AppConfiguration(ConfigurationEntity, Log));
        private readonly GetOnce<IAppConfiguration> _appConfig = new();

        public IEntity ConfigurationEntity => _appConfiguration ??= _appState.SettingsInApp.AppConfiguration;

        private IEntity _appConfiguration;

        #endregion


        public IImmutableList<IEntity> List => _appState.List;
        public IEntity GetDraft(IEntity entity) => _appState.GetDraft(entity);

        public IEntity GetPublished(IEntity entity) => _appState.GetPublished(entity);


        public IEnumerable<IContentType> ContentTypes => _appState.ContentTypes;

        public IContentType GetContentType(string name) => _appState.GetContentType(name);
        public IMetadataOf Metadata => _appState.Metadata;

        public AppRelationshipManager Relationships => _appState.Relationships;

        public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
            => _appState.GetMetadata(targetType, key, contentTypeName);

        public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null) 
            => _appState.GetMetadata(targetType, key, contentTypeName);


        #region Timestamps

        public long CacheTimestamp => _appState.CacheTimestamp;

        public bool CacheChanged(long dependentTimeStamp) => _appState.CacheChanged(dependentTimeStamp);

        #endregion


        IMetadataOf IMetadataOfSource.GetMetadataOf<T>(TargetTypes targetType, T key, string title = null) => _appState.GetMetadataOf(targetType, key, title);
    }
}
