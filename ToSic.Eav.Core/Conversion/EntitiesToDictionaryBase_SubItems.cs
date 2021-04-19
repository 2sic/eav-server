using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Conversion
{
    public abstract partial class EntitiesToDictionaryBase
    {
        private IEnumerable<RelationshipReference> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules) =>
            items.Select(e => new RelationshipReference
            {
                Id = rules.SerializeId == true ? e?.EntityId : null,
                Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
                Title = rules.SerializeTitle == true ? e?.GetBestTitle(Languages) : null
            });
    }
}
