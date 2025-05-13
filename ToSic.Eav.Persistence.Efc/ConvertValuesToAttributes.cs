using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ConvertValuesToAttributes(string primaryLanguage, ILog parentLog): HelperBase(parentLog, "Sql.CnvV2A")
{
    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpValueLoading
    //public Dictionary<int, IEnumerable<TempAttributeWithValues>> EavValuesToTempAttributes(List<ToSicEavValues> values)
    //{
    //    var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(timer: true);

    //    // Convert to dictionary
    //    // Research 2024-08 PC 2dm shows that this is superfast - less than 1ms for 1700 attributes (Tutorial App)
    //    var attributes = values
    //        .GroupBy(e => e.EntityId)
    //        .ToDictionary(
    //            e => e.Key,
    //            e => e
    //                .GroupBy(v => v.AttributeId)
    //                .Select(valueGroup => new TempAttributeWithValues
    //                {
    //                    Name = valueGroup.First().Attribute.StaticName,
    //                    Values = valueGroup
    //                        // The order of values is significant because the 2sxc system uses the first value as fallback
    //                        // Because we can't ensure order of values when saving, order values: prioritize values without
    //                        // any dimensions, then values with primary language
    //                        .OrderByDescending(v2 => !v2.TsDynDataValueDimensions.Any())
    //                        .ThenByDescending(v2 => v2.TsDynDataValueDimensions
    //                            .Any(lng => string.Equals(lng.Dimension.EnvironmentKey, primaryLanguage,
    //                                StringComparison.InvariantCultureIgnoreCase))
    //                        )
    //                        .ThenBy(v2 => v2.TransCreatedId)
    //                        .Select(v2 => new TempValueWithLanguage
    //                        {
    //                            Value = v2.Value,
    //                            Languages = v2.TsDynDataValueDimensions
    //                                .Select(ILanguage (lng) => new Language(lng.Dimension.EnvironmentKey, lng.ReadOnly,
    //                                    lng.DimensionId))
    //                                .ToImmutableList(),
    //                        })
    //                })
    //        );

    //    return l.Return(attributes);
    //}


    internal Dictionary<int, List<TempAttributeWithValues>> EavValuesToTempAttributesBeta(List<LoadingValue> allValues)
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>(timer: true);

        var primaryLower = primaryLanguage.ToLowerInvariant();

        // Convert to dictionary
        // Research 2024-08 PC 2dm shows that this is superfast - ca. 10-15ms for 1700 attributes (Tutorial App)
        var attributes = allValues
            .GroupBy(e => e.EntityId)
            .ToDictionary(
                e => e.Key,
                e => e
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
                            .Select(v => new TempValueWithLanguage { Value = v.Value, Languages = v.Languages.ToImmutableList() })
                            .ToList();



                        return new TempAttributeWithValues
                        {
                            Name = valueGroup.First().StaticName,
                            Values = values,
                        };
                    })
                    .ToList()
            );

        return l.Return(attributes);
    }

    //public Dictionary<int, IEnumerable<TempAttributeWithValues>> EavValuesToTempAttributesBeta(List<LoadingValue> values)
    //{
    //    var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(timer: true);

    //    var primaryLower = primaryLanguage.ToLowerInvariant();

    //    // Convert to dictionary
    //    // Research 2024-08 PC 2dm shows that this is superfast - less than 1ms for 1700 attributes (Tutorial App)
    //    var attributes = values
    //        .GroupBy(e => e.EntityId)
    //        .ToDictionary(
    //            e => e.Key,
    //            e => e
    //                .GroupBy(v => v.AttributeId)
    //                .Select(valueGroup => new TempAttributeWithValues
    //                {
    //                    Name = valueGroup.First(). /*Attribute.*/StaticName,
    //                    Values = valueGroup
    //                        // The order of values is significant because the 2sxc system uses the first value as fallback
    //                        // Because we can't ensure order of values when saving, order values: prioritize values without
    //                        // any dimensions, then values with primary language
    //                        .OrderByDescending(v2 => !v2.Dimensions.Any())
    //                        .ThenByDescending(v2 => v2.Dimensions.Any(lng => lng.Key == primaryLower)
    //                        )
    //                        .ThenBy(v2 => v2.TransCreatedId)
    //                        .Select(v2 => new TempValueWithLanguage
    //                        {
    //                            Value = v2.Value,
    //                            Languages = v2.Dimensions.ToImmutableList(),
    //                        })
    //                })
    //        );

    //    return l.Return(attributes);
    //}
}