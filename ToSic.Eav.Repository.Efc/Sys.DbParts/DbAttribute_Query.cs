namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

partial class DbAttribute
{

    /// <summary>
    /// Get Attributes of a Content-Type
    /// </summary>
    internal IQueryable<TsDynDataAttribute> GetAttributeDefinitions(int contentTypeId)
    {
        if (contentTypeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(contentTypeId), "should never be 0 - this is a bug because of the new Immutable, report to iJungleboy");

        contentTypeId = DbStore.ContentType.ResolvePotentialGhostContentTypeId(contentTypeId);

        return DbStore.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .Where(attributes => attributes.ContentTypeId == contentTypeId)
            .OrderBy(attributes => attributes.SortOrder);
    }


    /// <summary>
    /// Check if a valid, undeleted attribute-set exists
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <param name="staticName"></param>
    /// <returns></returns>
    private bool AttributeExistsInSet(int contentTypeId, string staticName)
        => DbStore.SqlDb.TsDynDataAttributes.Any(s =>
            s.StaticName == staticName
            && !s.TransDeletedId.HasValue
            && s.ContentTypeId == contentTypeId
            && s.ContentType.AppId == DbStore.AppId);


    // new parts
    public string[] DataTypeNames()
        => DbStore.SqlDb.TsDynDataAttributeTypes
            .OrderBy(a => a.Type)
            .Select(a => a.Type)
            .ToArray();

    public TsDynDataAttribute? GetTracked(int attributeId) 
        => DbStore.SqlDb.TsDynDataAttributes
            .FirstOrDefault(a => a.AttributeId == attributeId);
}