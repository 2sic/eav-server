using ToSic.Eav.Data.Dimensions.Sys;
using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ValueLoaderStandard(EfcAppLoader appLoader, EntityDetailsLoadSpecs specs, string logName = default)
    : HelperBase(appLoader.Log, logName ?? "Efc.VlLdSt")
{
    protected readonly EfcAppLoader AppLoader = appLoader;
    protected readonly EntityDetailsLoadSpecs Specs = specs;

    private ValueQueries ValueQueries => field ??= new(AppLoader.Context, Log);


    public virtual Dictionary<int, List<TempAttributeWithValues>> LoadValues()
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>(timer: true);

        var sqlTime = Stopwatch.StartNew();
        var result = Specs.IdsToLoadChunks
                .Select(GetValuesOfEntityChunk)
                .SelectMany(chunk => chunk);
        sqlTime.Stop();

        var attributes = result.ToDictionary(i => i.Key, i => i.Value);

        AppLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(attributes, $"Found {attributes.Count} attributes");
    }

    private Dictionary<int, List<TempAttributeWithValues>> GetValuesOfEntityChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<Dictionary<int, List<TempAttributeWithValues>>>($"ids: {entityIdsFound.Count}", timer: true);

        var attributesRaw = GetSqlValuesChunk(entityIdsFound);

        var attributes = ConvertToTempAttributes(attributesRaw);
        return l.ReturnAsOk(attributes);
    }

    /// <summary>
    /// Helper to convert LoadingValue list to TempAttributeWithValues dictionary.
    /// </summary>
    internal Dictionary<int, List<TempAttributeWithValues>> ConvertToTempAttributes(List<LoadingValue> attributesRaw)
    {
        var cnv = new ConvertValuesToAttributes(AppLoader.PrimaryLanguage, Log);
        return cnv.EavValuesToTempAttributesBeta(attributesRaw);
    }

    #region Chunked Mode / Single Access


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
        var attributesRaw = ToLoadingValues(query, null);

        return l.Return(attributesRaw, $"found {attributesRaw.Capacity} attributes");
    }

    /// <summary>
    /// Method to convert query results to LoadingValue list.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="dimensions"></param>
    /// <remarks>
    /// It's really important that we use an IQueryable here, because otherwise the
    /// SQL isn't properly converted and we'll run into null exceptions! So don't use IEnumerable,
    /// at least not for EF 2.2 which is still in use for DNN.
    /// </remarks>
    internal virtual List<LoadingValue> ToLoadingValues(IQueryable<TsDynDataValue> values, List<TsDynDataDimension> dimensions)
    {
        return values
            .ToList()
            .Select(v => new LoadingValue(
                v.EntityId,
                v.AttributeId,
                v.Attribute.StaticName,
                v.Value,
                v.TsDynDataValueDimensions
                    .Select(ILanguage (lng) =>
                        new Language(lng.Dimension.EnvironmentKey, lng.ReadOnly, lng.DimensionId))
                    .ToImmutableList()
            ))
            .ToList();
    }


    #endregion

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

}