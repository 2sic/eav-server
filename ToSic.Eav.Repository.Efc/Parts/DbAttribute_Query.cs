namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbAttribute
{

    /// <summary>
    /// Get Attributes of an AttributeSet
    /// </summary>
    internal IQueryable<ToSicEavAttributes> GetAttributeDefinitions(int attributeSetId)
    {
        if (attributeSetId <= 0)
            throw new ArgumentOutOfRangeException(nameof(attributeSetId), "should never be 0 - this is a bug because of the new Immutable, report to iJungleboy");

        attributeSetId = DbContext.ContentType.ResolvePotentialGhostAttributeSetId(attributeSetId);

        return DbContext.SqlDb.ToSicEavAttributesInSets
            .Where(ais => ais.AttributeSetId == attributeSetId)
            .OrderBy(ais => ais.SortOrder)
            .Select(ais => ais.Attribute);
    }


    /// <summary>
    /// Check if a valid, undeleted attribute-set exists
    /// </summary>
    /// <param name="attributeSetId"></param>
    /// <param name="staticName"></param>
    /// <returns></returns>
    private bool AttributeExistsInSet(int attributeSetId, string staticName)
        => DbContext.SqlDb.ToSicEavAttributesInSets.Any(s =>
            s.Attribute.StaticName == staticName
            && !s.Attribute.ChangeLogDeleted.HasValue
            && s.AttributeSetId == attributeSetId
            && s.AttributeSet.AppId == DbContext.AppId);


    // new parts
    public string[] DataTypeNames()
        => DbContext.SqlDb.ToSicEavAttributeTypes.OrderBy(a => a.Type)
            .Select(a => a.Type)
            .ToArray();

    public ToSicEavAttributes Get(int attributeId) 
        => DbContext.SqlDb.ToSicEavAttributes.FirstOrDefault(a => a.AttributeId == attributeId);
}