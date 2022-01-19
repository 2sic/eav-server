using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Apps;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    public abstract class SerializerBase: HasLog, IDataSerializer
    {
        #region Constructor / DI

        /// <summary>
        /// Constructor for inheriting classes
        /// </summary>
        protected SerializerBase(ITargetTypes metadataTargets, IAppStates appStates, string logName): base(logName)
        {
            MetadataTargets = metadataTargets;
            GlobalApp = appStates.GetPresetOrNull(); // important that it uses GlobalOrNull - because it may not be loaded yet
        }

        private readonly AppState GlobalApp;

        public ITargetTypes MetadataTargets { get; }


        public void Initialize(AppState appState, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            App = appState;
            AppId = appState.AppId;
        }

        protected int AppId;
        private IEnumerable<IContentType> _types;
        public void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities, ILog parentLog)
        {
            AppId = appId;
            _types = types;
            _relList = allEntities;
            Log.LinkTo(parentLog);
        }

        #endregion



        public AppState App
        {
            get => AppPackageOrNull ?? throw new Exception("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
            set => AppPackageOrNull = value;
        }
        protected AppState AppPackageOrNull { get; private set; }

        public bool PreferLocalAppTypes = false;

        protected IContentType GetContentType(string staticName)
        {
            var wrapLog = Log.Call<IContentType>($"name: {staticName}, preferLocal: {PreferLocalAppTypes}");

            // There is a complex lookup we must protocol, to better detect issues, which is why we assemble a message
            var msg = "";

            // If local type is preferred, use the App accessor,
            // this will also check the global types internally
            // ReSharper disable once InvertIf
            if (PreferLocalAppTypes)
            {
                var type = App.GetContentType(staticName);
                if (type != null) return wrapLog(msg + "app: found", type);
                msg += "app: not found, ";
            }

            var globalType = GlobalApp?.GetContentType(staticName);

            if (globalType != null) return wrapLog(msg + "global: found", globalType);
            msg += "global: not found, ";

            if (_types != null)
            {
                msg += "local-list: ";
                globalType = _types.FirstOrDefault(t => t.NameId == staticName);
            }
            else
            {
                msg += "app: ";
                globalType = App.GetContentType(staticName);
            }

            return wrapLog(msg + (globalType == null ? "not " : "") + "found", globalType);
        }


        protected IEntity Lookup(int entityId) => App.List.FindRepoId(entityId); // should use repo, as we're often serializing unpublished entities, and then the ID is the Repo-ID

        public abstract string Serialize(IEntity entity);

        public string Serialize(int entityId) => Serialize(Lookup(entityId));

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, Serialize);

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);

        protected IEntitiesSource LazyRelationshipLookupList => _relList ?? (_relList = App);
        private IEntitiesSource _relList;

    }
}
