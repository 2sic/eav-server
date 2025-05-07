namespace ToSic.Eav.Persistence.Efc;

internal class ValueQueries(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.ValQry")
{
    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpValueLoading
    ///// <remarks>
    ///// Research 2024-08 PC 2dm shows that this is fairly slow, between 100 and 400ms for 1700 attributes (Tutorial App)
    ///// </remarks>
    //internal IQueryable<ToSicEavValues> ValuesOfIdsQuery(List<int> entityIds)
    //{
    //    var l = Log.Fn<IQueryable<ToSicEavValues>>(timer: true);

    //    var query = context.ToSicEavValues
    //        // Note 2025-04-28 2dm: the .Attribute seems to only be used to get the StaticName
    //        // should probably be optimized to not get the Attribute definition for each item (lots of data)
    //        // but instead just get the StaticName
    //        .Include(v => v.Attribute)
    //        // Dimensions are needed for language assignment for each value
    //        .Include(v => v.ToSicEavValuesDimensions)
    //        .ThenInclude(d => d.Dimension)
    //        // Skip values which have been flagged as deleted
    //        .Where(v => !v.TransactionIdDeleted.HasValue);

    //    var queryOfEntityIds = query
    //        .Where(r => entityIds.Contains(r.EntityId));

    //    return l.Return(queryOfEntityIds);
    //}

    /// <remarks>
    /// Improved 2025-04-28 for v20 to really just get the values we need, seems to be ca. 50% faster.
    /// </remarks>
    internal IQueryable<TsDynDataValue> ValuesOfIdsQueryOptimized(List<int> entityIds)
    {
        var l = Log.Fn<IQueryable<TsDynDataValue>>(timer: true);

        var query = context.TsDynDataValues;
            // Skip values which have been flagged as deleted
            //.Where(v => !v.TransactionIdDeleted.HasValue);

        var queryOfEntityIds = query
            .Where(r => entityIds.Contains(r.EntityId));

        return l.Return(queryOfEntityIds);
    }

}