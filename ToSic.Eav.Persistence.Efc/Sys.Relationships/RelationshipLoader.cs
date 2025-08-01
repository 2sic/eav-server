using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Persistence.Efc.Sys.DbModels;
using ToSic.Eav.Persistence.Efc.Sys.Entities;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Persistence.Efc.Sys.TempModels;

#if NETFRAMEWORK
using ToSic.Sys.Utils;
#endif

namespace ToSic.Eav.Persistence.Efc.Sys.Relationships;

internal class RelationshipLoader(EfcAppLoaderService appLoader, EntityDetailsLoadSpecs specs) : HelperBase(appLoader.Log, "Efc.ValLdr")
{
    [field: AllowNull, MaybeNull]
    internal RelationshipQueries RelationshipQueries => field ??= new(appLoader, Log);

    public Dictionary<int, ICollection<TempRelationshipList>> LoadRelationships()
    {
        var l = Log.IfSummary(appLoader.LogSettings)
            .Fn<Dictionary<int, ICollection<TempRelationshipList>>>($"Chunk size: {specs.ChunkSize}; Chunks: {specs.IdsToLoadChunks.Count}", timer: true);

        // Load relationships in batches / chunks
        var sqlTime = Stopwatch.StartNew();
        var lRelationshipSql = Log.IfDetails(appLoader.LogSettings).Fn("Relationship SQL", timer: true);
        var relChunks = specs.IdsToLoadChunks
            .Select(idList => GetRelationshipChunkOptimizedSql(specs.AppId, idList))
            .SelectMany(chunk => chunk)
            .ToList();
        lRelationshipSql.Done();
        sqlTime.Stop();

        // in some strange cases we get duplicate keys - this should try to report what's happening
        var skipUniqueChecks = specs is { Optimized: true, IdsToLoadChunks.Count: 1 };
        var distinctRelationships = ReuniteChunkedRelationships(relChunks, skipUniqueChecks);
        var relatedEntities = GroupUniqueRelationshipsOptimized(distinctRelationships);

        appLoader.AddSqlTime(sqlTime.Elapsed);

        return l.Return(relatedEntities, $"Found {relChunks.Count} relationships; {distinctRelationships} unique; for {relatedEntities.Count} values in {sqlTime.ElapsedMilliseconds}ms; {appLoader.Context.TrackingInfo()}");
    }

    /// <summary>
    /// Get a chunk of relationships.
    /// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    /// See https://github.com/2sic/2sxc/issues/2127
    /// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    /// </summary>
    /// <returns></returns>
    private List<LoadingRelationship> GetRelationshipChunkOptimizedSql(int appId, List<int> entityIds)
    {
        var l = Log.IfDetails(appLoader.LogSettings).Fn<List<LoadingRelationship>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
        var relationships = RelationshipQueries
            .RelationshipChunkQueryOptimized(appId, entityIds)
            .Select(rel => new LoadingRelationship(rel, rel.Attribute.StaticName))
            .ToList();
        return l.Return(relationships, $"count: {relationships.Count}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, ICollection<TempRelationshipList>> GroupUniqueRelationshipsOptimized(ICollection<LoadingRelationship> unique)
    {
        var l = Log.IfDetails(appLoader.LogSettings)
            .Fn<Dictionary<int, ICollection<TempRelationshipList>>>($"items: {unique.Count}", timer: true);

        var relatedEntities = unique
            // First group by Parent ID
            .GroupBy(g => g.Rel.ParentEntityId)
            .ToDictionary(
                g => g.Key,
                ICollection<TempRelationshipList> (g) => g
                    // Now group by the exact field name (StaticName)
                    .GroupBy(r => r.StaticName)
                    .Select(loadGroup => new TempRelationshipList
                    {
                        StaticName = loadGroup.Key, // StaticName,
                        Children = loadGroup
                            .Select(lg => lg.Rel)
                            .OrderBy(c => c.SortOrder)
                            .Select(c => c.ChildEntityId)
                            .ToListOpt()
                    })
                    .ToListOpt());

        return l.Return(relatedEntities, $"count: {relatedEntities.Count}");
    }

    /// <param name="relationships">The relationships, possibly partially duplicated because of chunking</param>
    /// <param name="skipUniqueCheck">If the original only had one chunk, then it doesn't need the unique-check</param>
    private ICollection<LoadingRelationship> ReuniteChunkedRelationships(IReadOnlyCollection<LoadingRelationship> relationships, bool skipUniqueCheck)
    {
        var l = Log.IfDetails(appLoader.LogSettings)
            .Fn<ICollection<LoadingRelationship>>($"items: {relationships.Count}", timer: true);

        // Filter out duplicates, as the relationship manager doesn't need/want to count them, just establish relationship
        var comparer = new RelationshipComparer();
        var unique = skipUniqueCheck
            ? relationships.ToListOpt()
            : relationships
                .DistinctBy(x => x.Rel, comparer)
                .ToListOpt();
        
        return l.Return(unique, $"Final: {unique.Count}");
    }
}

internal record LoadingRelationship(TsDynDataRelationship Rel, string StaticName);


internal class RelationshipComparer : IEqualityComparer<TsDynDataRelationship>
{
    public bool Equals(TsDynDataRelationship? x, TsDynDataRelationship? y)
    {
        // inspired by ms docs https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1?view=net-9.0
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return x.AttributeId == y.AttributeId
               && x.SortOrder == y.SortOrder
               && x.ParentEntityId == y.ParentEntityId
               && x.ChildEntityId == y.ChildEntityId;
    }

    /// <summary>
    /// This is very important - it must return the same value for the similar objects to then be fully compared.
    /// Note that according to my research, an overflow will automatically start at negative maximum value.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(TsDynDataRelationship obj)
        => obj.ParentEntityId + obj.AttributeId + (obj.ChildEntityId ?? 0) + obj.SortOrder;
}