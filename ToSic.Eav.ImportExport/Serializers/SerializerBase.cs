using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public abstract class SerializerBase: IThingSerializer
    {
        public AppDataPackage App;

        public IThingSerializer Initialize(AppDataPackage app)
        {
            App = app;
            return this;
        }

        protected IEntity Lookup(int entityId)
        {
            if (App == null)
                throw new Exception($"Can't serialize entity {entityId} without the app package. Please initialize first, or provide a prepared entity");

            return App.Entities[entityId];
        }

        protected abstract string SerializeOne(IEntity entity);

        public string Serialize(int entityId) => SerializeOne(Lookup(entityId));
        public string Serialize(IEntity entity) => SerializeOne(entity);

        public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, x => Serialize(Lookup(x)));

        public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, e => SerializeOne(e).ToString());
    }
}
