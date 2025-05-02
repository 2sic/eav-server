namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbAttribute
{

    /// <summary>
    /// Get Attributes of an AttributeSet
    /// </summary>
    internal IQueryable<ToSicEavAttributes> GetAttributeDefinitions(int contentTypeId)
    {
        if (contentTypeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(contentTypeId), "should never be 0 - this is a bug because of the new Immutable, report to iJungleboy");

        contentTypeId = DbContext.ContentType.ResolvePotentialGhostAttributeSetId(contentTypeId);

        return DbContext.SqlDb.ToSicEavAttributes
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
        => DbContext.SqlDb.ToSicEavAttributes.Any(s =>
            s.StaticName == staticName
            && !s.TransactionIdDeleted.HasValue
            && s.ContentTypeId == contentTypeId
            && s.AttributeSet.AppId == DbContext.AppId);


    // new parts
    public string[] DataTypeNames()
        => DbContext.SqlDb.ToSicEavAttributeTypes.OrderBy(a => a.Type)
            .Select(a => a.Type)
            .ToArray();

    public ToSicEavAttributes Get(int attributeId) 
        => DbContext.SqlDb.ToSicEavAttributes.FirstOrDefault(a => a.AttributeId == attributeId);
}