namespace ToSic.Eav.RelationshipTests;

internal static class FilterStreamByIdsExtension
{

    /// <summary>
    /// Use this helper when you have a stream, but for testing need only a subset of the items in it. 
    /// 
    /// Will use a EntityIdFilter to achieve this
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="inStream"></param>
    /// <returns></returns>
    public static IDataStream FilterStreamByIds(this DataSourcesTstBuilder DsSvc, IEnumerable<int>? ids, IDataStream inStream)
    {
        if (ids == null || !ids.Any())
            return inStream;

        var entityFilterDs = DsSvc.CreateDataSource<EntityIdFilter>(inStream);
        entityFilterDs.EntityIds = string.Join(",", ids);
        inStream = entityFilterDs.GetStream();

        return inStream;
    }
}