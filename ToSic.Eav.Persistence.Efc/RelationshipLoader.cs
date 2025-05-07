using ToSic.Eav.Persistence.Efc.Intermediate;
#if NETFRAMEWORK
using ToSic.Lib.Internal.Generics;
#endif

namespace ToSic.Eav.Persistence.Efc;

internal class RelationshipLoader(EfcAppLoader appLoader, EntityDetailsLoadSpecs specs) : HelperBase(appLoader.Log, "Efc.ValLdr")
{
    internal RelationshipQueries RelationshipQueries => field ??= new(appLoader.Context, Log);

    public Dictionary<int, List<TempRelationshipList>> LoadRelationships()
    {
        var l = Log.Fn<Dictionary<int, List<TempRelationshipList>>>(timer: true);

        // Load relationships in batches / chunks
        var sqlTime = Stopwatch.StartNew();
        var lRelationshipSql = Log.Fn("Relationship SQL", timer: true);
        var relChunks = specs.IdsToLoadChunks
            .Select(idList => GetRelationshipChunkOptimizedSql(specs.AppId, idList))
            .SelectMany(chunk => chunk)
            .ToList();
        lRelationshipSql.Done();
        sqlTime.Stop();

        // in some strange cases we get duplicate keys - this should try to report what's happening
        var relatedEntities = GroupUniqueRelationshipsOptimized(relChunks);

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

    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpRelationshipLoading
    //private List<ToSicEavEntityRelationships> GetRelationshipChunk(int appId, ICollection<int> entityIds)
    //{
    //    var l = Log.Fn<List<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
    //    var relationships = RelationshipQueries
    //        .QueryRelationshipChunk(appId, entityIds)
    //        .ToList();
    //    return l.ReturnAsOk(relationships);
    //}
    private List<LoadingRelationship> GetRelationshipChunkOptimizedSql(int appId, List<int> entityIds)
    {
        var l = Log.Fn<List<LoadingRelationship>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
        var relationships = RelationshipQueries
            .RelationshipChunkQueryOptimized(appId, entityIds)
            .Select(rel => new LoadingRelationship(rel, rel.Attribute.StaticName))
            .ToList();
        return l.Return(relationships, $"count: {relationships.Count}");
    }

    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpRelationshipLoading
    //private Dictionary<int, List<TempRelationshipList>> GroupUniqueRelationships(IReadOnlyCollection<ToSicEavEntityRelationships> relationships)
    //{
    //    var l = Log.Fn<Dictionary<int, List<TempRelationshipList>>>($"items: {relationships.Count}", timer: true);

    //    l.A("experiment!");
    //    var unique = relationships.Distinct(new RelationshipComparer()).ToList();
    //    l.A("Distinct relationships: " + unique.Count);

    //    var relatedEntities = unique
    //        .GroupBy(g => g.ParentEntityId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.GroupBy(r => r.AttributeId)
    //                .Select(rg => new TempRelationshipList
    //                {
    //                    StaticName = rg.First().Attribute.StaticName,
    //                    Children = rg
    //                        .OrderBy(c => c.SortOrder)
    //                        .Select(c => c.ChildEntityId)
    //                        .ToList()
    //                })
    //                .ToList()
    //        );
    //    return l.ReturnAsOk(relatedEntities);
    //}
    private Dictionary<int, List<TempRelationshipList>> GroupUniqueRelationshipsOptimized(IReadOnlyCollection<LoadingRelationship> relationships)
    {
        var l = Log.Fn<Dictionary<int, List<TempRelationshipList>>>($"items: {relationships.Count}", timer: true);

        l.A("experiment!");
        // Filter out duplicates, as the relationship manager doesn't need/want to count them, just establish relationship
        var comparer = new RelationshipComparer();
        var unique = relationships
            .DistinctBy(x => x.Rel,  comparer)
            .ToList();
        l.A("Distinct relationships: " + unique.Count);

        var relatedEntities = unique
            // First group by Parent ID
            .GroupBy(g => g.Rel.ParentEntityId)
            .ToDictionary(
                g => g.Key,
                g => g
                    // Now group by the exact field - previously we grouped by AttributeId
                    .GroupBy(r => r.StaticName)
                    .Select(loadGroup =>
                    {
                        var rg = loadGroup.Select(lg => lg.Rel);
                        return new TempRelationshipList
                        {
                            StaticName = loadGroup.Key, //.First().StaticName,
                            Children = loadGroup
                                .Select(lg => lg.Rel)
                                .OrderBy(c => c.SortOrder)
                                .Select(c => c.ChildEntityId)
                                .ToList()
                        };
                    })
                    .ToList()
            );
        return l.Return(relatedEntities, $"count: {relatedEntities.Count}");
    }

}

internal record LoadingRelationship(TsDynDataRelationship Rel, string StaticName);


internal class RelationshipComparer : IEqualityComparer<TsDynDataRelationship>
{
    public bool Equals(TsDynDataRelationship x, TsDynDataRelationship y)
    {
        if (x == null && y == null) return true;
        if (x == null) return false;
        if (y == null) return false;
        return x.AttributeId == y.AttributeId
               && x.SortOrder == y.SortOrder
               && x.ParentEntityId == y.ParentEntityId
               && x.ChildEntityId == y.ChildEntityId;
    }

    public int GetHashCode(TsDynDataRelationship obj)
        => obj.GetHashCode();
}