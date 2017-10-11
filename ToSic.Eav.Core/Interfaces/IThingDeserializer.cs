using System.Collections.Generic;
using ToSic.Eav.App;

namespace ToSic.Eav.Interfaces
{
    public interface IThingDeserializer
    {
        void Initialize(AppDataPackage app);

        void Initialize(int appId, IEnumerable<IContentType> types);

        IEntity Deserialize(string serialized, bool allowDynamic = false);

        List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);
    }
}
