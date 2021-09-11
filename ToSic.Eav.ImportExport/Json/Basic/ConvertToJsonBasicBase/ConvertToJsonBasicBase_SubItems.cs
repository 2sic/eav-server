using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Json.Basic
{
    public abstract partial class ConvertToJsonBasicBase
    {
        private IEnumerable<JsonRelationship> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules) =>
            items.Select(e => new JsonRelationship
            {
                Id = rules.SerializeId == true ? e?.EntityId : null,
                Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
                Title = rules.SerializeTitle == true ? e?.GetBestTitle(Languages) : null
            });
    }
}
