using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Interfaces;

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

        public void Initialize(AppDataPackage app)
        {
            App = app;
            //return this;
        }

        protected IEntity Lookup(int entityId)
        {
            if (App == null)
                throw new Exception($"Can't serialize entity {entityId} without the app package. Please initialize first, or provide a prepared entity");

            return App.Entities[entityId];
        }

        public abstract string Serialize(IEntity entity);

        public string Serialize(int entityId) => Serialize(Lookup(entityId));

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, x => Serialize(Lookup(x)));

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);


    }
}
