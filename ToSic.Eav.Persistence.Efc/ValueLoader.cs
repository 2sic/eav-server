using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ValueLoader(EfcAppLoader appLoader, EntityDetailsLoadSpecs specs): HelperBase(appLoader.Log, "Efc.ValLdr")
{
    internal ValueQueries ValueQueries => field ??= new(appLoader.Context, Log);


    public Dictionary<int, List<TempAttributeWithValues>> LoadValues()
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>(timer: true);

        var sqlTime = Stopwatch.StartNew();
        var chunkedAttributes = specs.IdsToLoadChunks
            .Select(GetValuesOfEntityChunk);
        sqlTime.Stop();

        var attributes = chunkedAttributes
            .SelectMany(chunk => chunk)
            .ToDictionary(i => i.Key, i => i.Value);

        appLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(attributes, $"Found {attributes.Count} attributes");
    }

    private Dictionary<int, List<TempAttributeWithValues>> GetValuesOfEntityChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>($"ids: {entityIdsFound.Count}", timer: true);

        var attributesRaw = GetSqlValues(entityIdsFound);

        var cnv = new ConvertValuesToAttributes(appLoader.PrimaryLanguage, Log);
        var attributes = cnv.EavValuesToTempAttributesBeta(attributesRaw);

        return l.ReturnAsOk(attributes);
    }

    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpValueLoading
    ///// <summary>
    ///// Get the attributes of the entities we're loading.
    ///// </summary>
    ///// <param name="entityIdsFound"></param>
    ///// <returns></returns>
    ///// <remarks>
    ///// Research 2024-08 PC 2dm shows that this is fairly slow, between 100 and 400ms for 1700 attributes (Tutorial App)
    ///// </remarks>
    //private List<ToSicEavValues> GetSqlValuesRaw(List<int> entityIdsFound)
    //{
    //    var l = Log.Fn<List<ToSicEavValues>>($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

    //    var attributesRaw = ValueQueries
    //        .ValuesOfIdsQuery(entityIdsFound)
    //        .ToList();

    //    return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    //}

    /// <summary>
    /// Get the attributes of the entities we're loading.
    /// </summary>
    /// <param name="entityIdsFound"></param>
    /// <returns></returns>
    /// <remarks>
    /// Updated 2025-04-28 for v20 to really just get the values we need, seems to be ca. 50% faster.
    /// </remarks>
    private List<LoadingValue> GetSqlValues(List<int> entityIdsFound)
    {
        var l = Log.Fn<List<LoadingValue>>($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

        var attributesRaw = ValueQueries
            .ValuesOfIdsQueryOptimized(entityIdsFound)
            .Select(v => new LoadingValue(
                    v.EntityId,
                    v.AttributeId,
                    v.Attribute.StaticName,
                    v.Value,
                    v.ChangeLogCreated,
                    v.ToSicEavValuesDimensions
                    //null
                    //v.ToSicEavValuesDimensions
                    //    .Select(lng => new Language(lng.Dimension.EnvironmentKey, lng.ReadOnly, lng.DimensionId) as ILanguage)
                    //    .ToList()
                )
            )
            .ToList();

        return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    }
}