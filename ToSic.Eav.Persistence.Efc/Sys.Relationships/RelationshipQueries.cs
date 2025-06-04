namespace ToSic.Eav.Persistence.Efc;

internal class RelationshipQueries(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.RelQry")
{
    // 2025-04-28: this is the old version, which was slower - remove ca. 2025-Q3 #EfcSpeedUpRelationshipLoading
    ///// <summary>
    ///// Get a chunk of relationships.
    ///// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    ///// See https://github.com/2sic/2sxc/issues/2127
    ///// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    ///// </summary>
    ///// <returns></returns>
    //public IQueryable<ToSicEavEntityRelationships> QueryRelationshipChunk(int appId, ICollection<int> entityIds)
    //{
    //    var l = Log.Fn<IQueryable<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
    //    var relationships = context.ToSicEavEntityRelationships
    //        .Include(rel => rel.Attribute)
    //        .Where(rel => rel.ParentEntity.AppId == appId)
    //        .Where(r =>
    //                !r.ChildEntityId.HasValue // child can be a null-reference
    //                || entityIds.Contains(r.ChildEntityId.Value) // check if it's referred to as a child
    //                || entityIds.Contains(r.ParentEntityId) // check if it's referred to as a parent
    //        );
    //    return l.ReturnAsOk(relationships);
    //}

    /// <summary>
    /// Get a chunk of relationships.
    /// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    /// See https://github.com/2sic/2sxc/issues/2127
    /// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Optimized 2025-04-28 for v20 - not including Attributes any more by default.
    /// </remarks>
    public IQueryable<TsDynDataRelationship> RelationshipChunkQueryOptimized(int appId, List<int> entityIds)
    {
        var l = Log.Fn<IQueryable<TsDynDataRelationship>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
        var relationships = context.TsDynDataRelationships
            .Where(rel => rel.ParentEntity.AppId == appId)
            .Where(r =>
                    !r.ChildEntityId.HasValue // child can be a null-reference
                    || entityIds.Contains(r.ChildEntityId.Value) // check if it's referred to as a child
                    || entityIds.Contains(r.ParentEntityId) // check if it's referred to as a parent
            );
        return l.ReturnAsOk(relationships);
    }

}