using ToSic.Eav.Persistence.Efc.Sys.TempModels;

namespace ToSic.Eav.Persistence.Efc.Sys.Values;

internal class ConvertValuesToAttributes(string primaryLanguage, ILog? parentLog): HelperBase(parentLog, "Sql.CnvV2A")
{

    internal Dictionary<int, ICollection<TempAttributeWithValues>> EavValuesToTempAttributesBeta(ICollection<LoadingValue> allValues)
    {
        var l = Log.Fn<Dictionary<int, ICollection<TempAttributeWithValues>>>(timer: true);

        var primaryLower = primaryLanguage.ToLowerInvariant();

        // Convert to dictionary
        // Research 2024-08 PC 2dm shows that this is superfast - ca. 10-15ms for 1700 attributes (Tutorial App)
        var attributes = allValues
            .GroupBy(e => e.EntityId)
            .ToDictionary(
                e => e.Key, ICollection<TempAttributeWithValues> (e) => e
                    .GroupBy(v => v.AttributeId)
                    .Select(valueGroup =>
                    {
                        var values = valueGroup
                            .Select(v => new
                            {
                                v.Value,
                                v.Languages,
                            })
                            // The order of values is significant because the 2sxc system uses the first value as fallback
                            // Because we can't ensure order of values when saving, order values: prioritize values without
                            // any dimensions, then values with primary language
                            .OrderByDescending(v2 => !v2.Languages.Any())
                            .ThenByDescending(v2 => v2.Languages.Any(lng => lng.Key == primaryLower))
                            .Select(v => new TempValueWithLanguage { Value = v.Value, Languages = v.Languages.ToImmutableOpt() })
                            .ToListOpt();



                        return new TempAttributeWithValues
                        {
                            Name = valueGroup.First().StaticName,
                            Values = values,
                        };
                    })
                    .ToListOpt());

        return l.Return(attributes);
    }

}