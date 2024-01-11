namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{
    /// <summary>
    /// Delete an Entity
    /// </summary>
    internal bool DeleteEntity(int repositoryId, bool autoSave = true, bool removeFromParents = false) 
        => DeleteEntity([repositoryId], autoSave, removeFromParents);

    internal bool DeleteEntity(int[] repositoryId, bool autoSave = true, bool removeFromParents = false)
    {
        Log.A($"DeleteEntity(rep-ids:{repositoryId.Length}, remove-from-parents:{removeFromParents}, auto-save:{autoSave})");
        if (repositoryId.Length == 0 || repositoryId.Contains(0))
            return false;

        // get full entity again to be sure we are deleting everything - otherwise inbound unreliable
        // note that as this is a DB-entity, the EntityId is actually the repositoryId
        var entities = DbContext.Entities.GetDbEntities(repositoryId, "ToSicEavValues,ToSicEavValues.ToSicEavValuesDimensions");


        #region Delete Related Records (Values, Value-Dimensions, Relationships)
        // Delete all Value-Dimensions
        var valueDimensions = entities.SelectMany(e => e.ToSicEavValues.SelectMany(v => v.ToSicEavValuesDimensions)).ToList();
        DbContext.SqlDb.RemoveRange(valueDimensions);

        // Delete all Values
        DbContext.SqlDb.RemoveRange(entities.SelectMany(e => e.ToSicEavValues).ToList());

        // Delete all Parent-Relationships
        DeleteRelationships(entities.SelectMany(e => e.RelationshipsWithThisAsParent).ToList());
        if (removeFromParents)
            DeleteRelationships(entities.SelectMany(e => e.RelationshipsWithThisAsChild).ToList());

        #endregion

        var draftBranchMap = DbContext.Publishing.GetDraftBranchMap(entities.Select(e => e.EntityId).ToList());

        entities.ForEach(entity =>
        {
            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                Log.A("was published, will mark as deleted");
                entity.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
                // Also delete the Draft (if any)
                draftBranchMap.TryGetValue(entity.EntityId, out var draftEntityId);
                //var draftEntityId = DbContext.Publishing.GetDraftBranchEntityId(entity.EntityId);
                if (draftEntityId.HasValue) DeleteEntity(draftEntityId.Value);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                Log.A("was draft, will really delete");
                // Delete all Child-Relationships
                DeleteRelationships(entity.RelationshipsWithThisAsChild);
                DbContext.SqlDb.Remove(entity);
            }
        });
        if (autoSave)
            DbContext.SqlDb.SaveChanges();

        Log.A("DeleteEntity(...) done");

        return true;
    }

    private void DeleteRelationships(ICollection<ToSicEavEntityRelationships> relationships)
    {
        Log.A($"DeleteRelationships({relationships?.Count})");
        if ((relationships?.Count ?? 0) == 0)
            Log.A("No relationships to delete");
        else
            relationships?.ToList().ForEach(r => DbContext.SqlDb.ToSicEavEntityRelationships.Remove(r));
        Log.A("/DeleteRelationships(...)");
    }
        
}