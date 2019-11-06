using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Interfaces
{
    public interface IThingDeserializer: IHasLog
    {
        void Initialize(AppDataPackage app, ILog parentLog);

        void Initialize(int appId, IEnumerable<IContentType> types, IDeferredEntitiesList allEntities, ILog parentLog);

        IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false);

        List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);
    }
}
