using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace

namespace ToSic.Eav.Persistence.Xml
{
    public abstract class DeserializerBase : IThingDeserializer
    {
        public AppDataPackage App;

        public void Initialize(AppDataPackage app)
        {
            App = app;
        }

        public abstract IEntity Deserialize(string serialized);

        public List<IEntity> Deserialize(List<string> serialized) => serialized.Select(Deserialize).ToList();
    }
}
