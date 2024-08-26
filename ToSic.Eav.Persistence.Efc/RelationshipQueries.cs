namespace ToSic.Eav.Persistence.Efc;

internal class RelationshipQueries(EavDbContext context, ILog parentLog): HelperBase(parentLog, "Efc.RelQry")
{
    /// <summary>
    /// Get a chunk of relationships.
    /// Note that since it must check child/parents then multiple chunks could return the identical relationship.
    /// See https://github.com/2sic/2sxc/issues/2127
    /// This is why the conversion to dictionary etc. must happen later, when all chunks are merged.
    /// </summary>
    /// <returns></returns>
    public IQueryable<ToSicEavEntityRelationships> QueryRelationshipChunk(int appId, ICollection<int> entityIds)
    {
        var l = Log.Fn<IQueryable<ToSicEavEntityRelationships>>($"app: {appId}, ids: {entityIds.Count}", timer: true);
        var relationships = context.ToSicEavEntityRelationships
            .Include(rel => rel.Attribute)
            .Where(rel => rel.ParentEntity.AppId == appId)
            .Where(r =>
                    !r.ChildEntityId.HasValue // child can be a null-reference
                    || entityIds.Contains(r.ChildEntityId.Value) // check if it's referred to as a child
                    || entityIds.Contains(r.ParentEntityId) // check if it's referred to as a parent
            );
        return l.ReturnAsOk(relationships);
    }


}