using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Metadata;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using ToSic.Sxc.Apps;

namespace ToSic.Eav.Apps.Services
{
    internal class AppStateDataService() : ServiceBase("App.Reader"), IAppStateInternal, IMetadataSource
    {
        internal AppStateDataService Init(IAppStateCache appState, ILog parentLog)
        {
            _appState = appState as AppState;
            this.LinkLog(parentLog);
            return this;
        }
        private AppState _appState;

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

        public IAppConfiguration Configuration => _appConfig.Get(() => new AppConfiguration(ConfigurationEntity, Log));
        private readonly GetOnce<IAppConfiguration> _appConfig = new();

        public IEntity ConfigurationEntity => _appConfiguration ??= _appState.SettingsInApp.AppConfiguration;

        private IEntity _appConfiguration;

        #endregion

        #region PiggyBack

        PiggyBack IHasPiggyBack.PiggyBack => _appState.PiggyBack;

        #endregion

        #region Internal


        IAppStateCache IAppStateInternal.StateCache => _appState;
        IAppStateCache IAppStateInternal.ParentAppState => _appState.ParentApp?.AppState;
        SynchronizedEntityList IAppStateInternal.ListCache => _appState.ListCache;

        SynchronizedList<IEntity> IAppStateInternal.ListPublished => _appState.ListPublished;

        SynchronizedList<IEntity> IAppStateInternal.ListNotHavingDrafts => _appState.ListNotHavingDrafts;
        AppStateMetadata IAppStateInternal.SettingsInApp => _appState.SettingsInApp;

        AppStateMetadata IAppStateInternal.ResourcesInApp => _appState.ResourcesInApp;

        ParentAppState IAppStateInternal.ParentApp => _appState.ParentApp;

        AppRelationshipManager IAppStateInternal.Relationships => _appState.Relationships;

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
    }
}
