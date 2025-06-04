using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys.Options;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// Converter of lightweight sub-entities containing at most id/guid/title.
/// </summary>
/// <param name="languages"></param>
internal class ConvertToJsonBasicLightSubEntities(string[] languages)
{
    internal object CreateListOrCsvOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules)
    {
        var format = rules.SerializeFormat;
        
        // Default / classic case: return the list of Reference objects
        if (format.IsEmptyOrWs() || format == "object")
            return CreateListOfSubEntities(items, rules);

        // New case v15.03 - return CSV of Guids
        if (rules.SerializeGuid == true)
        {
            var guids = items.Select(i => i.EntityGuid.ToString());
            return format == "csv"
                ? string.Join(",", guids)
                : guids.ToArray();
        }

        // New case v15.03 - return CSV of IDs
        var ids = items.Select(i => i.EntityId);
        return format == "csv"
            ? string.Join(",", ids)
            : ids.ToArray();
    }

    internal IEnumerable<EavLightEntityReference> CreateListOfSubEntities(IEnumerable<IEntity> items, ISubEntitySerialization rules)
        => items.Select(e => new EavLightEntityReference
        {
            Id = rules.SerializeId == true ? e?.EntityId : null,
            Guid = rules.SerializeGuid == true ? e?.EntityGuid : null,
            Title = rules.SerializeTitle == true ? e?.GetBestTitle(languages) : null
        });
}