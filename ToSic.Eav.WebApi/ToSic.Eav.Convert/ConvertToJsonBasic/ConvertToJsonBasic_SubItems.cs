using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.Basic;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    public partial class ConvertToJsonBasic
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
