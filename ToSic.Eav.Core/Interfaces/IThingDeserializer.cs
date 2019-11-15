using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Interfaces
{
    public interface IThingDeserializer: IHasLog
    {
        void Initialize(AppState app, ILog parentLog);

        void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities, ILog parentLog);

        Data.IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false);

        List<Data.IEntity> Deserialize(List<string> serialized, bool allowDynamic = false);
    }
}
