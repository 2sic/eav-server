using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Interfaces
{
    public interface IThingDeserializer: IHasLog
    {
        void Initialize(AppDataPackage app, Log parentLog);

        void Initialize(int appId, IEnumerable<IContentType> types, IDeferredEntitiesList allEntities, Log parentLog);

        IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false);

        List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);
    }
}
