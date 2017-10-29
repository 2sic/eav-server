using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public abstract class SerializerBase: IThingSerializer
    {
        public AppDataPackage App
        {
            get => _app ?? throw new Exception("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
            set => _app = value;
        }
        private AppDataPackage _app;

        public IContentType GetContentType(string staticName)
            => Global.SystemContentType(staticName) // note: will return null if not found
               ?? (_types != null
                   ? _types.FirstOrDefault(t => t.StaticName == staticName)
                   : App.GetContentType(staticName)); // only use the App if really necessary...

        public void Initialize(AppDataPackage app)
        {
            App = app;
            AppId = app.AppId;
        }

        protected int AppId;
        private IEnumerable<IContentType> _types;
        public void Initialize(int appId, IEnumerable<IContentType> types, IDeferredEntitiesList allEntities)
        {
            AppId = appId;
            _types = types;
            _relList = allEntities;
        }

        protected IEntity Lookup(int entityId) => App.Entities[entityId];

        public abstract string Serialize(IEntity entity);

        public string Serialize(int entityId) => Serialize(Lookup(entityId));

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, x => Serialize(Lookup(x)));

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);



        protected IDeferredEntitiesList RelLookupList
        {
            get
            {
                if (_relList != null) return _relList;
                var appList = new AppDataPackageDeferredList();
                appList.AttachApp(App);
                _relList = appList;
                return _relList;
            }
        }
        private IDeferredEntitiesList _relList;


    }
}
