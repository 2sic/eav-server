using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Types;
using AppState = ToSic.Eav.Apps.AppState;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    public abstract class SerializerBase: HasLog, IDataSerializer
    {
        /// <summary>
        /// Empty constructor for DI
        /// </summary>
        protected SerializerBase() : this("Srl.Default") { }

        /// <summary>
        /// Normal constructor
        /// </summary>
        /// <param name="name"></param>
        protected SerializerBase(string name): base(name) { }


        public AppState App
        {
            get => AppPackageOrNull ?? throw new Exception("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
            set => AppPackageOrNull = value;
        }
        protected AppState AppPackageOrNull { get; private set; }

        public bool PreferLocalAppTypes = false;

        public IContentType GetContentType(string staticName)
        {
            // If local type is preferred, use the App accessor,
            // this will also check the global types internally
            // ReSharper disable once InvertIf
            if (PreferLocalAppTypes)
            {
                var type = App.GetContentType(staticName);
                if (type != null) return type;
            }

            return Global.FindContentType(staticName) // note: will return null if not found
                       ?? (_types != null
                           ? _types.FirstOrDefault(t => t.StaticName == staticName)
                           : App.GetContentType(staticName));
        }

        public void Initialize(AppState appState, ILog parentLog)
        {
            App = appState;
            AppId = appState.AppId;
            Log.LinkTo(parentLog);
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

        protected IEntity Lookup(int entityId) => App.List.FindRepoId(entityId); // should use repo, as we're often serializing unpublished entities, and then the ID is the Repo-ID

        public abstract string Serialize(IEntity entity);

        public string Serialize(int entityId) => Serialize(Lookup(entityId));

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, Serialize);

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);

        protected IEntitiesSource LazyRelationshipLookupList => _relList ?? (_relList = App);
        private IEntitiesSource _relList;

        protected int GetMetadataNumber(string name) => MetadataProvider.GetId(name);

        protected string GetMetadataName(int id) => MetadataProvider.GetName(id);

        public ITargetTypes MetadataProvider =>
            _mdProvider ?? (_mdProvider = Factory.Resolve<ITargetTypes>());
        private ITargetTypes _mdProvider;
    }
}
