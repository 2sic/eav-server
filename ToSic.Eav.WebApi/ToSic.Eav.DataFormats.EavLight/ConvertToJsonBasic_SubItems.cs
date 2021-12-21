using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    public partial class ConvertToEavLight
    {
        internal IEnumerable<EavLightEntityReference> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules) =>
            items.Select(e => new EavLightEntityReference
            {
                Id = rules.SerializeId == true ? e?.EntityId : null,
                Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
                Title = rules.SerializeTitle == true ? e?.GetBestTitle(Languages) : null
            });
    }
}
