using ToSic.Sys.Data;
using ToSic.Sys.Documentation;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    private int DeleteTransactionId => _deleteTransactionId.Get(() => DbStore.Versioning.GetTransactionId());
    private readonly GetOnce<int> _deleteTransactionId = new();

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
        var entities = DbStore.Entities.GetDbEntitiesFullUntracked(repositoryId);

        #region Delete Related Records (Values, Value-Dimensions, Relationships)

        // Delete all Value-Dimensions
        var valueDimensions = entities
            .SelectMany(e => e.TsDynDataValues.SelectMany(v => v.TsDynDataValueDimensions))
            .ToList();
        DbStore.SqlDb.RemoveRange(valueDimensions);

        // Delete all Values
        DbStore.SqlDb.RemoveRange(entities.SelectMany(e => e.TsDynDataValues).ToListOpt());

        // Delete all Parent-Relationships
        //DbStore.SqlDb.RemoveRange(entities.SelectMany(e => e.RelationshipsWithThisAsParent).ToListOpt());
        var relationshipsWithThisAsParentToSoftDelete = entities
            .SelectMany(e => e.RelationshipsWithThisAsParent)
            .ToListOpt();

        entities.ForEach(entity =>
        {
            foreach (var relationship in entity.RelationshipsWithThisAsParent)
            {
                var childEntityGuid = DbStore.Entities.GetDbEntitiesFullUntracked([relationship!.ChildEntityId!.Value])
                    .First().EntityGuid;
                relationship.ChildExternalId = childEntityGuid;
                relationship.ChildEntityId = null;
                relationship.TransDeletedId = DeleteTransactionId;
            }
        });

        DbStore.SqlDb.UpdateRange(relationshipsWithThisAsParentToSoftDelete);

        //if (removeFromParents)
        //    DbStore.SqlDb.RemoveRange(entities.SelectMany(e => e.RelationshipsWithThisAsChild).ToListOpt());

        if (removeFromParents)
        {
            var relationshipsWithThisAsChildToSoftDelete = entities
                .SelectMany(e => e.RelationshipsWithThisAsChild)
                .ToListOpt();

            entities.ForEach(entity =>
            {
                foreach (var relationship in entity.RelationshipsWithThisAsChild)
                {
                    relationship.ChildExternalId = entity.EntityGuid;
                    relationship.ChildEntityId = null;
                    relationship.TransDeletedId = DeleteTransactionId;
                }
            });

            DbStore.SqlDb.UpdateRange(relationshipsWithThisAsChildToSoftDelete);
        }

        #endregion

        var draftBranchMap = DbStore.Publishing.GetDraftBranchMap(entities.Select(e => e.EntityId).ToList());

        entities.ForEach(entity =>
        {
            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                l.A("was published, will mark as deleted");
                entity.TransDeletedId = DeleteTransactionId;
                // Also delete the Draft (if any) - but don't auto-save, as we would do that below
                draftBranchMap.TryGetValue(entity.EntityId, out var draftEntityId);
                if (draftEntityId.HasValue)
                    DeleteEntities([draftEntityId.Value], autoSave: false);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                l.A("was draft, will really delete");
                // Delete all Child-Relationships
                if (!removeFromParents)
                    DbStore.SqlDb.RemoveRange(entity.RelationshipsWithThisAsChild.ToListOpt());
            }

            // Also remove the entity itself
            MarkEntityAsDeletedTrackedOrUntracked(entity);
        });
        if (autoSave)
            DbStore.DoAndSaveWithoutChangeDetection(() => {});

        return l.ReturnTrue("DeleteEntity(...) done"); ;
    }

    private void MarkEntityAsDeletedTrackedOrUntracked(TsDynDataEntity entity)
    {
        // Guard against tracking conflicts: if the context already tracks another instance of this key,
        // update that tracked instance instead of attaching this untracked one.
        var localTracked = DbStore.SqlDb.TsDynDataEntities.Local
            .FirstOrDefault(e => e.EntityId == entity.EntityId);
        if (localTracked != null && !ReferenceEquals(localTracked, entity))
        {
            if (entity.TransDeletedId != localTracked.TransDeletedId)
                localTracked.TransDeletedId = entity.TransDeletedId;

            DbStore.SqlDb.Entry(localTracked).Property(e => e.TransDeletedId).IsModified = true;
            return;
        }

        DbStore.SqlDb.Attach(entity);
        DbStore.SqlDb.Entry(entity).Property(e => e.TransDeletedId).IsModified = true;
    }

    //private void RemoveEntityTrackedOrUntracked(TsDynDataEntity entity)
    //{
    //    // Guard against tracking conflicts: if the context already tracks another instance of this key,
    //    // delete that tracked instance instead of attaching this untracked one.
    //    var localTracked = DbStore.SqlDb.TsDynDataEntities.Local.FirstOrDefault(e => e.EntityId == entity.EntityId);
    //    if (localTracked != null && !ReferenceEquals(localTracked, entity))
    //    {
    //        // ensure the deleted flag is copied if it was set on the untracked instance
    //        if (entity.TransDeletedId != localTracked.TransDeletedId)
    //            localTracked.TransDeletedId = entity.TransDeletedId;

    //        DbStore.SqlDb.Entry(localTracked).State = EntityState.Deleted;
    //    }
    //    else
    //        DbStore.SqlDb.Remove(entity);
    //}

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