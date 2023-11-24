using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

public partial class ConvertToEavLight
{
    internal object CreateListOrCsvOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules)
    {
        // Default / classic case: return the list of Reference objects
        if (rules.SerializesAsCsv != true)
            return CreateListOfSubEntities(items, rules);

        // New case v15.03 - return CSV of IDs
        var ids = rules.SerializeGuid == true
            ? items.Select(i => i.EntityGuid.ToString())
            : items.Select(i => i.EntityId.ToString());
        var strIds = string.Join(",", ids);
        return strIds;

    }

    internal IEnumerable<EavLightEntityReference> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules) =>
        items.Select(e => new EavLightEntityReference
        {
            Id = rules.SerializeId == true ? e?.EntityId : null,
            Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
            Title = rules.SerializeTitle == true ? e?.GetBestTitle(Languages) : null
        });
}