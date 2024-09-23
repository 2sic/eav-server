namespace ToSic.Eav.Persistence.Efc;

internal class ValueQueries(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.ValQry")
{
    /// <summary>
    /// Get the attributes of the entities we're loading.
    /// </summary>
    private IQueryable<ToSicEavValues> AllWithDimensions()
    {
        var l = Log.Fn<IQueryable<ToSicEavValues>>(timer: true);

        var query = context.ToSicEavValues
            .Include(v => v.Attribute)
            .Include(v => v.ToSicEavValuesDimensions)
            .ThenInclude(d => d.Dimension)
            .Where(v => !v.ChangeLogDeleted.HasValue);

        return l.Return(query);
    }

    /// <remarks>
    /// Research 2024-08 PC 2dm shows that this is fairly slow, between 100 and 400ms for 1700 attributes (Tutorial App)
    /// </remarks>
    public IQueryable<ToSicEavValues> ValuesOfIds(List<int> entityIds)
    {
        var l = Log.Fn<IQueryable<ToSicEavValues>>(timer: true);

        var query = AllWithDimensions()
            .Where(r => entityIds.Contains(r.EntityId));

        return l.Return(query);
    }

    ///// <remarks>
    ///// NOT tested / proved to be usefull
    ///// </remarks>
    //public IQueryable<ToSicEavValues> ValuesOfInnerSelect(IQueryable<int> innerSelect)
    //{
    //    var l = Log.Fn<IQueryable<ToSicEavValues>>(timer: true);

    //    var query = AllWithDimensions()
    //        .Where(r => innerSelect.Contains(r.EntityId));

    //    return l.Return(query);
    //}

}