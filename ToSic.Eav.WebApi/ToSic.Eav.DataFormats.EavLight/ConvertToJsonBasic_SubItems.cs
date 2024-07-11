using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{
    internal object CreateListOrCsvOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules)
    {
        var format = rules.SerializeFormat;
        // Default / classic case: return the list of Reference objects
        //if (rules.SerializesAsCsv != true)
        if (format.IsEmptyOrWs() || format == "object")
            return CreateListOfSubEntities(items, rules);

        // New case v15.03 - return CSV of IDs
        if (rules.SerializeGuid == true)
        {
            var guids = items.Select(i => i.EntityGuid.ToString());
            return format == "csv" // rules.SerializeListAsString != false
                ? string.Join(",", guids)
                : guids.ToArray();
        }

        var ids = items.Select(i => i.EntityId);
        return format == "csv" //  rules.SerializeListAsString != false
            ? string.Join(",", ids)
            : ids.ToArray();
    }

    internal IEnumerable<EavLightEntityReference> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules) =>
        items.Select(e => new EavLightEntityReference
        {
            Id = rules.SerializeId == true ? e?.EntityId : null,
            Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
            Title = rules.SerializeTitle == true ? e?.GetBestTitle(Languages) : null
        });
}