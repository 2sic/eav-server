using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Data;

namespace ToSic.Eav.Interfaces
{
    public interface IThingSerializer
    {
        //IEntitySerializer Initialize(int appId);
        IThingSerializer Initialize(AppDataPackage app);


        string Serialize(int entityId);
        Dictionary<int, string> Serialize(List<int> entities);

        string Serialize(IEntity entity);
        Dictionary<int, string> Serialize(List<IEntity> entities);
    }
}
