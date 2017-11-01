using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IItemMetadata: IEnumerable<IEntity>
    {
        void Add(string type, Dictionary<string, object> values);
        void Use(List<IEntity> items);
    }
}
