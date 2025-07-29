namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    /// <summary>
    /// Delete one or more Entities
    /// </summary>
    internal bool DeleteEntities(int[] repositoryId, bool autoSave = true, bool removeFromParents = false)
    {
        var l = LogDetails.Fn<bool>($"DeleteEntity(rep-ids:{repositoryId.Length}, remove-from-parents:{removeFromParents}, auto-save:{autoSave})");
        if (repositoryId.Length == 0 || repositoryId.Contains(0))
            return l.ReturnFalse();

        // get full entity again to be sure we are deleting everything - otherwise inbound unreliable
        // note that as this is a DB-entity, the EntityId is actually the repositoryId
        var entities = DbContext.Entities.GetDbEntitiesFull(repositoryId);


        #region Delete Related Records (Values, Value-Dimensions, Relationships)

        // Delete all Value-Dimensions
        var valueDimensions = entities
            .SelectMany(e => e.TsDynDataValues.SelectMany(v => v.TsDynDataValueDimensions))
            .ToList();
        DbContext.SqlDb.RemoveRange(valueDimensions);

        // Delete all Values
        DbContext.SqlDb.RemoveRange(entities.SelectMany(e => e.TsDynDataValues).ToListOpt());

        // Delete all Parent-Relationships
        DbContext.SqlDb.RemoveRange(entities.SelectMany(e => e.RelationshipsWithThisAsParent).ToListOpt());
        if (removeFromParents)
            DbContext.SqlDb.RemoveRange(entities.SelectMany(e => e.RelationshipsWithThisAsChild).ToListOpt());

        #endregion

        var draftBranchMap = DbContext.Publishing.GetDraftBranchMap(entities.Select(e => e.EntityId).ToList());

        entities.ForEach(entity =>
        {
            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                l.A("was published, will mark as deleted");
                entity.TransDeletedId = DbContext.Versioning.GetTransactionId();
                // Also delete the Draft (if any)
                draftBranchMap.TryGetValue(entity.EntityId, out var draftEntityId);
                //var draftEntityId = DbContext.Publishing.GetDraftBranchEntityId(entity.EntityId);
                if (draftEntityId.HasValue)
                    DeleteEntities([draftEntityId.Value]);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                l.A("was draft, will really delete");
                // Delete all Child-Relationships
                DbContext.SqlDb.RemoveRange(entity.RelationshipsWithThisAsChild.ToListOpt());
                DbContext.SqlDb.Remove(entity);
            }
        });
        if (autoSave)
            DbContext.DoAndSaveWithoutChangeDetection(() => {});

        return l.ReturnTrue("DeleteEntity(...) done"); ;
    }

    //private void DeleteRelationshipsUntracked(ICollection<TsDynDataRelationship> relationships)
    //{
    //    var l = Log.Fn($"DeleteRelationships({relationships?.Count})");
    //    if ((relationships?.Count ?? 0) == 0)
    //        l.A("No relationships to delete");
    //    else
    //        DbContext.SqlDb.RemoveRange(relationships.ToList());
    //    l.Done();
    //}
        
}