using System.Collections.Generic;
using ToSic.Eav.App;

namespace ToSic.Eav.Interfaces
{
    public interface IThingDeserializer
    {
        IThingDeserializer Initialize(AppDataPackage app);

        IEntity Deserialize(string serialized);

        List<IEntity> Deserialize(List<string> serialized);
    }
}
