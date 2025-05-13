using ToSic.Eav.Internal.Features;
using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ValueLoader(EfcAppLoader appLoader, EntityDetailsLoadSpecs specs, IEavFeaturesService features) : HelperBase(appLoader.Log, "Efc.ValLdr")
{
    internal ValueQueries ValueQueries => field ??= new(appLoader.Context, Log);


    public Dictionary<int, List<TempAttributeWithValues>> LoadValues()
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>(timer: true);

        var sqlTime = Stopwatch.StartNew();
        var result = features.IsEnabled(BuiltInFeatures.SqlLoadPerformance)
            ? GetValuesOfAllEntitiesInApp(appId: specs.AppId)
            : specs.IdsToLoadChunks.Select(GetValuesOfEntityChunk).SelectMany(chunk => chunk);
        sqlTime.Stop();

        var attributes = result.ToDictionary(i => i.Key, i => i.Value);

        appLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(attributes, $"Found {attributes.Count} attributes");
    }

    private Dictionary<int, List<TempAttributeWithValues>> GetValuesOfEntityChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>($"ids: {entityIdsFound.Count}", timer: true);

        var attributesRaw = GetSqlValuesChunk(entityIdsFound);

        var attributes = ConvertToTempAttributes(attributesRaw);
        return l.ReturnAsOk(attributes);
    }

    private Dictionary<int, List<TempAttributeWithValues>> GetValuesOfAllEntitiesInApp(int appId)
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>($"appId:{appId}", timer: true);

        var attributesRaw = GetSqlValuesAll(appId);

        var attributes = ConvertToTempAttributes(attributesRaw);
        return l.ReturnAsOk(attributes);
    }

    /// <summary>
    /// Helper to convert LoadingValue list to TempAttributeWithValues dictionary.
    /// </summary>
    private Dictionary<int, List<TempAttributeWithValues>> ConvertToTempAttributes(List<LoadingValue> attributesRaw)
    {
        var cnv = new ConvertValuesToAttributes(appLoader.PrimaryLanguage, Log);
        return cnv.EavValuesToTempAttributesBeta(attributesRaw);
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
    private List<LoadingValue> GetSqlValuesChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<List<LoadingValue>>($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

        var query = ValueQueries.ChunkValuesQuery(entityIdsFound);
        var attributesRaw = ToLoadingValues(query);

        return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    }

    /// <summary>
    /// Get the attributes of the entities we're loading.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    private List<LoadingValue> GetSqlValuesAll(int appId)
    {
        var l = Log.Fn<List<LoadingValue>>($"Attributes SQL for appId:{appId}", timer: true);

        var query = ValueQueries.AllValuesQuery(appId);
        var attributesRaw = ToLoadingValues(query);

        return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    }

    // Reusable method to convert query results to LoadingValue list
    private List<LoadingValue> ToLoadingValues(IEnumerable<TsDynDataValue> values)
    {
        return values
            .Select(v => new LoadingValue(
                v.EntityId,
                v.AttributeId,
                v.Attribute.StaticName,
                v.Value,
                v.TsDynDataValueDimensions
                    .Select(lng =>
                        new Language(lng.Dimension.EnvironmentKey, lng.ReadOnly, lng.DimensionId) as ILanguage)
                    .ToList()
            ))
            .ToList();
    }

    #region Test
    // 2025-05-12, stv, test alternative strategy to load values
    //private List<LoadingValue> GetSqlValues2(List<int> entityIdsFound)
    //{
    //    var l = Log.Fn<List<LoadingValue>>($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

    //    var attributesRaw = ValueQueries
    //        .ValuesOfIdsQueryOptimized2(entityIdsFound)
    //        .Select(v => new LoadingValue(
    //                v.EntityId,
    //                v.AttributeId,
    //                v.StaticName,
    //                v.Value,

    //                v.ValueDimensionResults
    //                    .Select(lng =>
    //                        new Language(lng.EnvironmentKey, lng.ReadOnly, lng.DimensionId) as ILanguage)
    //                    .ToImmutableList()
    //            )
    //        )
    //        .ToList();

    //    return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    //}

    #endregion
}