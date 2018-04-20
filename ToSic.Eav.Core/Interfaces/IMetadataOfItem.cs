using System.Collections.Generic;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Interfaces
{
    public interface IMetadataOfItem: IEnumerable<IEntity>, IHasPermissions
    {
        //// 2018-03-09 2dm - this was used when we tried creating code-based content-types, but I believe it's dead code now
        void Add(string type, Dictionary<string, object> values);
        void Add(IEntity additionalItem);
        void Use(List<IEntity> items);

    }
}
