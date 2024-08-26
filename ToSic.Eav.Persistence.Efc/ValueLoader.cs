using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class ValueLoader(Efc11Loader parent, EntityDetailsLoadSpecs specs): HelperBase(parent.Log, "Efc.ValLdr")
{
    internal ValueQueries ValueQueries => _valueQueries ??= new(parent.Context, Log);
    private ValueQueries _valueQueries;


    public Dictionary<int, IEnumerable<TempAttributeWithValues>> Load(Stopwatch sqlTime)
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>(timer: true);

        sqlTime.Start();
        var chunkedAttributes = specs.IdsToLoadChunks
            .Select(GetValuesOfEntityChunk);
        sqlTime.Stop();

        var attributes = chunkedAttributes
            .SelectMany(chunk => chunk)
            .ToDictionary(i => i.Key, i => i.Value);

        return l.Return(attributes, $"Found {attributes.Count} attributes");
    }

    private Dictionary<int, IEnumerable<TempAttributeWithValues>> GetValuesOfEntityChunk(List<int> entityIdsFound)
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempAttributeWithValues>>>($"ids: {entityIdsFound.Count}", timer: true);

        var attributesRaw = GetSqlValuesRaw(entityIdsFound);

        var cnv = new ConvertValuesToAttributes(parent.PrimaryLanguage, Log);
        var attributes = cnv.EavValuesToTempAttributes(attributesRaw);

        return l.ReturnAsOk(attributes);
    }

    /// <summary>
    /// Get the attributes of the entities we're loading.
    /// </summary>
    /// <param name="entityIdsFound"></param>
    /// <returns></returns>
    /// <remarks>
    /// Research 2024-08 PC 2dm shows that this is fairly slow, between 100 and 400ms for 1700 attributes (Tutorial App)
    /// </remarks>
    private List<ToSicEavValues> GetSqlValuesRaw(List<int> entityIdsFound)
    {
        var lSql = Log.Fn($"Attributes SQL for {entityIdsFound.Count} entities", timer: true);

        var attributesRaw = ValueQueries
            .ValuesOfIds(entityIdsFound)
            .ToList();

        lSql.Done($"found {attributesRaw.Capacity} attributes");
        return attributesRaw;
    }

    ///// <summary>
    ///// Get the attributes of the entities we're loading.
    ///// Experimental, probably shouldn't be used yet.
    /////
    ///// Would only make sense to use in certain conditions, e.g. when loading all entities of an app.
    /////
    ///// DO NOT USE YET - it has not been proven to work or be faster
    ///// </summary>
    //private List<ToSicEavValues> GetSqlValuesRawInnerSelect()
    //{
    //    var lSql = Log.Fn($"Attributes SQL for entities", timer: true);

    //    var attributesRaw = parent.ValueQueries
    //        .ValuesOfInnerSelect(
    //            parent.EntityQueries.AppEntities(specs.AppId, [])
    //                .Select(e => e.EntityId)
    //        )
    //        .ToList();

    //    lSql.Done($"found {attributesRaw.Capacity} attributes");
    //    return attributesRaw;
    //}


}