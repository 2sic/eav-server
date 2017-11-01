using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IMetadataOfItem: IEnumerable<IEntity>
    {
        void Add(string type, Dictionary<string, object> values);
        void Add(IEntity additionalItem);
        void Use(List<IEntity> items);
    }
}
