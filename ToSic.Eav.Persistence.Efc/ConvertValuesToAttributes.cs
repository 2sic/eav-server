using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ConvertValuesToAttributes(string primaryLanguage, ILog parentLog): HelperBase(parentLog, "Sql.CnvV2A")
{

    public Dictionary<int, IEnumerable<TempAttributeWithValues>> EavValuesToTempAttributes(List<ToSicEavValues> values)
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(timer: true);

        // Convert to dictionary
        // Research 2024-08 PC 2dm shows that this is superfast - less than 1ms for 1700 attributes (Tutorial App)
        var attributes = values
            .GroupBy(e => e.EntityId)
            .ToDictionary(e => e.Key, e => e.GroupBy(v => v.AttributeId)
                .Select(vg => new TempAttributeWithValues
                {
                    Name = vg.First().Attribute.StaticName,
                    Values = vg
                        // The order of values is significant because the 2sxc system uses the first value as fallback
                        // Because we can't ensure order of values when saving, order values: prioritize values without
                        // any dimensions, then values with primary language
                        .OrderByDescending(v2 => !v2.ToSicEavValuesDimensions.Any())
                        .ThenByDescending(v2 => v2.ToSicEavValuesDimensions.Any(l =>
                            string.Equals(l.Dimension.EnvironmentKey, primaryLanguage,
                                StringComparison.InvariantCultureIgnoreCase)))
                        .ThenBy(v2 => v2.ChangeLogCreated)
                        .Select(v2 => new TempValueWithLanguage
                        {
                            Value = v2.Value,
                            Languages = v2.ToSicEavValuesDimensions
                                .Select(l => new Language(l.Dimension.EnvironmentKey, l.ReadOnly, l.DimensionId) as ILanguage)
                                .ToImmutableList(),
                        })
                }));

        return l.Return(attributes);
    }
}