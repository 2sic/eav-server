using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        /// <summary>
        /// Delete an Entity
        /// </summary>
        internal bool DeleteEntity(int repositoryId, bool autoSave = true, bool removeFromParents = false)
        {
            // 2020-10-09 Switched to batch-delete processing for performance reasons
            return DeleteEntity(new[] {repositoryId}, autoSave, removeFromParents);
            //Log.Add($"DeleteEntity(rep-id:{repositoryId}, remove-from-parents:{removeFromParents}, auto-save:{autoSave})");
            //if (repositoryId == 0)
            //    return false;

            //// get full entity again to be sure we are deleting everything - otherwise inbound unreliable
            //// note that as this is a DB-entity, the EntityId is actually the repositoryId
            //var entity = DbContext.Entities.GetDbEntity(repositoryId, "ToSicEavValues,ToSicEavValues.ToSicEavValuesDimensions");


            //#region Delete Related Records (Values, Value-Dimensions, Relationships)
            //// Delete all Value-Dimensions
            //var valueDimensions = entity.ToSicEavValues.SelectMany(v => v.ToSicEavValuesDimensions).ToList();
            //DbContext.SqlDb.RemoveRange(valueDimensions);

            //// Delete all Values
            //DbContext.SqlDb.RemoveRange(entity.ToSicEavValues.ToList());

            //// Delete all Parent-Relationships
            //DeleteRelationships(entity.RelationshipsWithThisAsParent);
            //if (removeFromParents)
            //    DeleteRelationships(entity.RelationshipsWithThisAsChild);

            //#endregion

            //// If entity was Published, set Deleted-Flag
            //if (entity.IsPublished)
            //{
            //    Log.Add("was published, will mark as deleted");
            //    entity.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            //    // Also delete the Draft (if any)
            //    var draftEntityId = DbContext.Publishing.GetDraftBranchEntityId(entity.EntityId);
            //    if (draftEntityId.HasValue)
            //        DeleteEntity(draftEntityId.Value);
            //}
            //// If entity was a Draft, really delete that Entity
            //else
            //{
            //    Log.Add("was draft, will really delete");
            //    // Delete all Child-Relationships
            //    DeleteRelationships(entity.RelationshipsWithThisAsChild);
            //    DbContext.SqlDb.Remove(entity);
            //}

            //if (autoSave)
            //    DbContext.SqlDb.SaveChanges();

            //Log.Add("DeleteEntity(...) done");

            //return true;
        }

        internal bool DeleteEntity(int[] repositoryId, bool autoSave = true, bool removeFromParents = false)
        {
            Log.Add($"DeleteEntity(rep-ids:{repositoryId.Length}, remove-from-parents:{removeFromParents}, auto-save:{autoSave})");
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
                    Log.Add("was published, will mark as deleted");
                    entity.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
                    // Also delete the Draft (if any)
                    draftBranchMap.TryGetValue(entity.EntityId, out var draftEntityId);
                    //var draftEntityId = DbContext.Publishing.GetDraftBranchEntityId(entity.EntityId);
                    if (draftEntityId.HasValue) DeleteEntity(draftEntityId.Value);
                }
                // If entity was a Draft, really delete that Entity
                else
                {
                    Log.Add("was draft, will really delete");
                    // Delete all Child-Relationships
                    DeleteRelationships(entity.RelationshipsWithThisAsChild);
                    DbContext.SqlDb.Remove(entity);
                }
            });
            if (autoSave)
                DbContext.SqlDb.SaveChanges();

            Log.Add("DeleteEntity(...) done");

            return true;
        }

        private void DeleteRelationships(ICollection<ToSicEavEntityRelationships> relationships)
        {
            Log.Add($"DeleteRelationships({relationships?.Count})");
            if ((relationships?.Count ?? 0) == 0)
                Log.Add("No relationships to delete");
            else
                relationships?.ToList().ForEach(r => DbContext.SqlDb.ToSicEavEntityRelationships.Remove(r));
            Log.Add("/DeleteRelationships(...)");
        }

        // Commented in v13, new implementation is based on AppState.Relationships.
        //internal Dictionary<int, Tuple<bool, string>> CanDeleteEntityBasedOnDbRelationships(int[] entityIds)
        //{
        //    var callLog = Log.Call($"can delete entity i:{entityIds.Length}", useTimer: true);
        //    var entities = GetDbEntities(entityIds);

        //    var result = new Dictionary<int, Tuple<bool, string>>();

        //    foreach (var e in entities)
        //        if (!e.IsPublished && e.PublishedEntityId == null) // always allow Deleting Draft-Only Entity 
        //            result.Add(e.EntityId, new Tuple<bool, string>(true, null));
        //    //return new Tuple<bool, string>(true, null);

        //    var rest = entities.Where(e => !result.ContainsKey(e.EntityId)).ToList();
        //    var restIds = rest.Select(r => r.EntityId).Cast<int?>().ToList();

        //    var allParents = DbContext.SqlDb.ToSicEavEntityRelationships
        //        .Where(r => restIds.Contains(r.ChildEntityId))
        //        .Select(r => new TempEntityAndTypeInfos
        //        {
        //            Target = r.ChildEntityId ?? -1,
        //            EntityId = r.ParentEntityId,
        //            TypeId = r.ParentEntity.AttributeSetId
        //        })
        //        .ToList();
        //    foreach (var entity in rest)
        //    {
        //        var entityId = entity.EntityId;
        //        var messages = new List<string>();

        //        #region check if there are relationships where this is a child
        //        //var parents = DbContext.SqlDb.ToSicEavEntityRelationships
        //        //    .Where(r => r.ChildEntityId == entityId)
        //        //    .Select(r => new TempEntityAndTypeInfos { EntityId = r.ParentEntityId, TypeId = r.ParentEntity.AttributeSetId })
        //        //    .ToList();
        //        var parents = allParents.Where(p => p.Target == entityId).ToList();
        //        if (parents.Any())
        //        {
        //            TryToGetMoreInfosAboutDependencies(parents, messages);
        //            messages.Add($"found {parents.Count} relationships where this is a child - the parents are: {string.Join(", ", parents)}.");
        //        }
        //        #endregion

        //        // TODO: This doesn't look right - entity-assignments should always be guid, not int - so this is probably wrong
        //        // Must verify and then change to use the guid instead

        //        var entitiesAssignedToThis = GetAssignedEntities((int)TargetTypes.Entity, entityId)
        //            .Select(e => new TempEntityAndTypeInfos { EntityId = e.EntityId, TypeId = e.AttributeSetId })
        //            .ToList();
        //        if (entitiesAssignedToThis.Any())
        //        {
        //            TryToGetMoreInfosAboutDependencies(entitiesAssignedToThis, messages);
        //            messages.Add($"found {entitiesAssignedToThis.Count} entities which are metadata for this, assigned children (like in a pipeline) or assigned for other reasons: {string.Join(", ", entitiesAssignedToThis)}.");
        //        }
        //        result.Add(entityId, Tuple.Create(!messages.Any(), string.Join(" ", messages)));
        //    }
        //    // var entityId = entityIds.First();
        //    if (result.Count != entityIds.Length)
        //        throw new Exception("Delete check failed, results doesn't match request");
        //    callLog("ok");
        //    return result;
        //}

        //private void TryToGetMoreInfosAboutDependencies(IEnumerable<TempEntityAndTypeInfos> dependencies, List<string> messages)
        //{
        //    try
        //    {
        //        // try to get more infos about the parents
        //        foreach (var dependency in dependencies)
        //            dependency.TypeName = DbContext.AttribSet.GetDbAttribSet(dependency.TypeId).Name;
        //    }
        //    catch
        //    {
        //        messages.Add("Relationships but was not able to look up more details to show a nicer error.");
        //    }
        //}

        //private class TempEntityAndTypeInfos
        //{
        //    internal int Target;


        //    internal int EntityId;
        //    internal int TypeId;
        //    internal string TypeName = "";

        //    public override string ToString() => EntityId + " (" + TypeName + ")";

        //}
    }
}
