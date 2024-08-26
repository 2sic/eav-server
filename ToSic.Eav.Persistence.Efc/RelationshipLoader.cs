using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class RelationshipLoader(EfcAppLoader appLoader, EntityDetailsLoadSpecs specs) : HelperBase(appLoader.Log, "Efc.ValLdr")
{
    internal RelationshipQueries RelationshipQueries => _relationshipQueries ??= new(appLoader.Context, Log);
    private RelationshipQueries _relationshipQueries;

    public Dictionary<int, IEnumerable<TempRelationshipList>> LoadRelationships()
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempRelationshipList>>>(timer: true);

        // Load relationships in batches / chunks
        var sqlTime = Stopwatch.StartNew();
        var lRelationshipSql = Log.Fn("Relationship SQL", timer: true);
        var relChunks = specs.IdsToLoadChunks
            .Select(idList => GetRelationshipChunk(specs.AppId, idList))
            .SelectMany(chunk => chunk)
            .ToList();
        lRelationshipSql.Done();
        sqlTime.Stop();

        // in some strange cases we get duplicate keys - this should try to report what's happening
        var relatedEntities = GroupUniqueRelationships(relChunks);

        appLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(relatedEntities, $"Found {relatedEntities.Count} entity relationships in {sqlTime.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Get a chunk of relationships.
    /// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    /// See https://github.com/2sic/2sxc/issues/2127
    /// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    /// </summary>
    /// <returns></returns>
    private List<ToSicEavEntityRelationships> GetRelationshipChunk(int appId, ICollection<int> entityIds)
    {
        var l = Log.Fn<List<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
        var relationships = RelationshipQueries
            .QueryRelationshipChunk(appId, entityIds)
            .ToList();
        return l.ReturnAsOk(relationships);
    }

    private Dictionary<int, IEnumerable<TempRelationshipList>> GroupUniqueRelationships(IReadOnlyCollection<ToSicEavEntityRelationships> relationships)
    {
        var l = Log.Fn<Dictionary<int, IEnumerable<TempRelationshipList>>>($"items: {relationships.Count}", timer: true);

        l.A("experiment!");
        var unique = relationships.Distinct(new RelationshipComparer()).ToList();
        l.A("Distinct relationships: " + unique.Count);

        var relatedEntities = unique
            .GroupBy(g => g.ParentEntityId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.AttributeId)
                    .Select(rg => new TempRelationshipList
                    {
                        StaticName = rg.First().Attribute.StaticName,
                        Children = rg
                            .OrderBy(c => c.SortOrder)
                            .Select(c => c.ChildEntityId)
                            .ToList()
                    }));
        return l.ReturnAsOk(relatedEntities);
    }

}



internal class RelationshipComparer : IEqualityComparer<ToSicEavEntityRelationships>
{
    public bool Equals(ToSicEavEntityRelationships x, ToSicEavEntityRelationships y)
    {
        if (x == null && y == null) return true;
        if (x == null) return false;
        if (y == null) return false;
        return x.AttributeId == y.AttributeId
               && x.SortOrder == y.SortOrder
               && x.ParentEntityId == y.ParentEntityId
               && x.ChildEntityId == y.ChildEntityId;
    }

    public int GetHashCode(ToSicEavEntityRelationships obj)
        => obj.GetHashCode();
}