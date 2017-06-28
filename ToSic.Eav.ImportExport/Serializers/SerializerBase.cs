using System.Collections.Generic;
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

        public abstract string Serialize(int entityId);
        public abstract Dictionary<int, string> Serialize(List<int> entities);


        public abstract string Serialize(IEntity entity);

        public abstract Dictionary<int, string> Serialize(List<IEntity> entities);
    }
}
