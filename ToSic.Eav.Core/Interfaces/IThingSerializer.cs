using System.Collections.Generic;
using ToSic.Eav.App;

namespace ToSic.Eav.Interfaces
{
    public interface IThingSerializer
    {
        IThingSerializer Initialize(AppDataPackage app);

        string Serialize(int entityId);
        Dictionary<int, string> Serialize(List<int> entities);

        string Serialize(IEntity entity);

        Dictionary<int, string> Serialize(List<IEntity> entities);
    }
}
