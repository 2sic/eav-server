using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Interfaces
{
    public interface IThingSerializer
    {
        void Initialize(AppDataPackage app, Log parentLog);

        //void Initialize(int appId, IEnumerable<IContentType> types);

        string Serialize(int entityId);
        Dictionary<int, string> Serialize(List<int> entities);

        string Serialize(IEntity entity);

        Dictionary<int, string> Serialize(List<IEntity> entities);


    }
}
