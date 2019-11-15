using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Interfaces
{
    public interface IThingSerializer
    {
        void Initialize(AppState app, ILog parentLog);

        //void Initialize(int appId, IEnumerable<IContentType> types);

        string Serialize(int entityId);
        Dictionary<int, string> Serialize(List<int> entities);

        string Serialize(Data.IEntity entity);

        Dictionary<int, string> Serialize(List<Data.IEntity> entities);


    }
}
