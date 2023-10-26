using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    public abstract class SerializerBase: ServiceBase<SerializerBase.MyServices>, IDataSerializer
    {
        #region Constructor / DI

        public class MyServices: MyServicesBase
        {
            public DataBuilder DataBuilder { get; }

            public MyServices(ITargetTypes metadataTargets, DataBuilder dataBuilder, IAppStates appStates)
            {
                ConnectServices(
                    MetadataTargets = metadataTargets,
                    DataBuilder = dataBuilder,
                    AppStates = appStates
                );
            }

            public ITargetTypes MetadataTargets { get; }
            public IAppStates AppStates { get; }
        }

        /// <summary>
        /// Constructor for inheriting classes
        /// </summary>
        protected SerializerBase(MyServices services, string logName): base(services, logName)
        {
            MetadataTargets = services.MetadataTargets;
            _globalApp = services.AppStates.GetPresetOrNull(); // important that it uses GlobalOrNull - because it may not be loaded yet
        }
        private readonly AppState _globalApp;

        public ITargetTypes MetadataTargets { get; }


        public void Initialize(AppState appState)
        {
            App = appState;
            AppId = appState.AppId;
        }

        protected int AppId;
        private IEnumerable<IContentType> _types;
        public void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities)
        {
            AppId = appId;
            _types = types;
            _relList = allEntities;
        }

        #endregion



        public AppState App
        {
            get => AppOrNull ?? throw new Exception("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
            set => AppOrNull = value;
        }
        protected AppState AppOrNull { get; private set; }

        public bool PreferLocalAppTypes = false;

        protected IContentType GetContentType(string staticName
        ) => Log.Func($"name: {staticName}, preferLocal: {PreferLocalAppTypes}", () =>
        {
            // There is a complex lookup we must protocol, to better detect issues, which is why we assemble a message
            var msg = "";

            // If local type is preferred, use the App accessor,
            // this will also check the global types internally
            // ReSharper disable once InvertIf
            if (PreferLocalAppTypes)
            {
                var type = App.GetContentType(staticName);
                if (type != null) return (type, $"app: found");
                msg += "app: not found, ";
            }

            var globalType = _globalApp?.GetContentType(staticName);

            if (globalType != null) return (globalType, $"{msg}global: found");
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

            return (globalType, $"{msg}{(globalType == null ? "not " : "")}found");
        });

        protected IContentType GetTransientContentType(string name, string nameId)
        {
            var defaultTransient = Services.DataBuilder.ContentType.Transient(AppId, name, nameId);
            return ContentTypeProvider?.LazyTypeGenerator(AppId, name, nameId, defaultTransient)
                   ?? defaultTransient;
        }
        /// <summary>
        /// Ability to inject a different TransientContentTypeGenerator
        /// </summary>
        public IDeferredContentTypeProvider ContentTypeProvider { get; set; } = null;

        protected IEntity Lookup(int entityId) => App.List.FindRepoId(entityId); // should use repo, as we're often serializing unpublished entities, and then the ID is the Repo-ID

        public abstract string Serialize(IEntity entity);

        public string Serialize(int entityId) => Serialize(Lookup(entityId));

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, Serialize);

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);

        protected IEntitiesSource LazyRelationshipLookupList => _relList ?? (_relList = App);
        private IEntitiesSource _relList;

    }
}
