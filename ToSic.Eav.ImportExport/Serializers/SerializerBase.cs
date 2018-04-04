using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data.Query;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Types;

namespace ToSic.Eav.ImportExport.Serializers
{
    public abstract class SerializerBase: HasLog, IThingSerializer
    {
        protected SerializerBase(string name): base(name) { }

        protected SerializerBase() : this("Srl.Default") { }

        public AppDataPackage App
        {
            get => _app ?? throw new Exception("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
            set => _app = value;
        }
        private AppDataPackage _app;
        protected AppDataPackage AppPackageOrNull => _app;

        public IContentType GetContentType(string staticName)
            => Global.FindContentType(staticName) // note: will return null if not found
               ?? (_types != null
                   ? _types.FirstOrDefault(t => t.StaticName == staticName)
                   : App.GetContentType(staticName)); // only use the App if really necessary...

        public void Initialize(AppDataPackage app, Log parentLog)
        {
            App = app;
            AppId = app.AppId;
            Log.LinkTo(parentLog);
        }

        protected int AppId;
        private IEnumerable<IContentType> _types;
        public void Initialize(int appId, IEnumerable<IContentType> types, IDeferredEntitiesList allEntities, Log parentLog)
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



        protected IDeferredEntitiesList RelLookupList
        {
            get
            {
                if (_relList != null) return _relList;
                //var appList = new AppDataPackageDeferredList();
                //appList.AttachApp(App);
                _relList = App; // appList;
                return _relList;
            }
        }
        private IDeferredEntitiesList _relList;


    }
}
