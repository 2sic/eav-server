using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {

        public int SaveEntity(IEntity newEnt, SaveOptions so)
        {

            #region Step 1: Do some initial error checking and preparations
            if (newEnt == null)
                throw new ArgumentNullException(nameof(newEnt));

            if (newEnt.Type == null)
                throw new Exception("trying to save entity without known content-type, cannot continue");

            #region Test what languages are given, and check if they exist in the target system
            var usedLanguages = newEnt.GetUsedLanguages();

            var zoneLanguages = DbContext.Dimensions.GetLanguages();

            if(usedLanguages.Count > 0)
                if (!usedLanguages.All(l => zoneLanguages.Any(zl => zl.EnvironmentKey.ToLowerInvariant() == l.Key)))
                    throw new Exception("found languages in save which are not available in environment");
            #endregion Test languages exist

            #endregion Step 1



            #region Step 2: check header record - does it already exist, what ID should we use, etc.

            // If we think we'll update an existing entity...
            // ...we have to check if we'll actualy update the draft of the entity
            // ...or create a new draft (branch)
            int? existingDraftId = null;
            bool hasAdditionalDraft = false;
            if (newEnt.EntityId > 0)
            {
                existingDraftId = DbContext.Publishing.GetDraftBranchEntityId(newEnt.EntityId);  // find a draft of this - note that it won't find anything, if the item itself is the draft
                hasAdditionalDraft = existingDraftId != null && existingDraftId.Value != newEnt.EntityId;  // only true, if there is an "attached" draft; false if the item itself is draft
                if (!newEnt.IsPublished && ((Entity) newEnt).PlaceDraftInBranch)
                {
                    ((Entity)newEnt).SetPublishedIdForSaving(newEnt.EntityId);  // set this, in case we'll create a new one
                    ((Entity) newEnt).ChangeIdForSaving(existingDraftId ?? 0);  // set to the draft OR 0 = new
                    hasAdditionalDraft = false; // not additional any more, as we're now pointing this as primary
                }
            }
            var isNew = newEnt.EntityId <= 0;   // no remember how we want to work...

            var contentTypeId = DbContext.AttribSet.GetIdWithEitherName(newEnt.Type.StaticName);
            var attributeDefs = DbContext.AttributesDefinition.GetAttributeDefinitions(contentTypeId).ToList();

            #endregion Step 2



            ToSicEavEntities dbEnt;
            // create a transaction, but only if none is already running around it
            var ownTransaction = DbContext.SqlDb.Database.CurrentTransaction == null ? DbContext.SqlDb.Database.BeginTransaction() : null;

            var changeId = DbContext.Versioning.GetChangeLogId();
            try
            {

                #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

                if (isNew)
                {
                    #region Step 3a: Create new

                    dbEnt = new ToSicEavEntities
                    {
                        AssignmentObjectTypeId = newEnt.Metadata?.TargetType ?? Constants.NotMetadata,
                        KeyNumber = newEnt.Metadata?.KeyNumber,
                        KeyGuid = newEnt.Metadata?.KeyGuid,
                        KeyString = newEnt.Metadata?.KeyString,
                        ChangeLogCreated = changeId,
                        ChangeLogModified = changeId,
                        EntityGuid = newEnt.EntityGuid != Guid.Empty ? newEnt.EntityGuid : Guid.NewGuid(),
                        IsPublished = newEnt.IsPublished,
                        PublishedEntityId = newEnt.IsPublished ? null : ((Entity)newEnt).GetPublishedIdForSaving(),
                        Owner = DbContext.UserName,
                        AttributeSetId = contentTypeId
                    };

                    DbContext.SqlDb.Add(dbEnt);
                    DbContext.SqlDb.SaveChanges();
                    #endregion
                }
                else
                {
                    #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                    // todo: check if repo-id is always there, may need to use repoid OR entityId
                    // new: always change the draft if there is one! - it will then either get published, or not...
                    dbEnt = DbContext.Entities.GetDbEntity(newEnt.EntityId);

                    var publishedStateChangesForThisItem = dbEnt.IsPublished != newEnt.IsPublished;
                    #region Unpublished Save (Draft-Saves) - do some possible error checking

                    #region removed publish/unpublish code
                    // Prevent editing of Published item if there's a draft
                    //if (dbEnt.IsPublished && dbEntAttachedDraft.HasValue)
                    //    throw new Exception($"Update Entity not allowed because a draft exists with EntityId {dbEntAttachedDraft}");

                    // Current Entity is published but Update as a draft
                    //if (dbEnt.IsPublished && !newEnt.IsPublished && so.AllowBranching)
                    //    // Prevent duplicate Draft
                    //    throw existingDraftId.HasValue
                    //        ? new InvalidOperationException(
                    //            $"Published EntityId {dbId} has already a draft with EntityId {existingDraftId}")
                    //        : new InvalidOperationException(
                    //            "It seems you're trying to update a published entity with a draft - this is not possible - the save should actually try to create a new draft instead without calling update.");

                    // if the published state is going to draft, but there is already a draft, the wrong item was edited
                    // basically when editing a draft, then this is also the item that should receive the save command
                    //if (dbEnt.IsPublished && !newEnt.IsPublished && draftItemId.HasValue)
                    //    throw new InvalidOperationException($"Published EntityId {dbId} already has draft {draftItemId}, so can't create new");

                    #endregion removed

                    #endregion

                    #region If draft but should be published, correct what's necessary

                    // Update as Published but Current Entity is a Draft-Entity
                    // case 1: saved entity is a draft and save wants to publish
                    // case 2: new data is set to not publish, but we don't want a branch
                    if (publishedStateChangesForThisItem || hasAdditionalDraft)
                    {
                        // if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                        if (dbEnt.PublishedEntityId.HasValue)
                            DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, false);

                        // now reset the branch/entity-state to properly set the state / purge the draft
                        dbEnt = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt, existingDraftId, newEnt.IsPublished); 

                        ((Entity)newEnt).ChangeIdForSaving(dbEnt.EntityId);   // update ID of the save-entity, as it's used again later on...
                    }

                    #endregion

                    #endregion Step 3b
                }

                #endregion Step 3

                #region Step 4: Save all normal values
                // first, clean up all existing attributes / values (flush)
                dbEnt.ToSicEavValues.Clear();
                DbContext.SqlDb.SaveChanges();  // this is necessary after remove, because otherwise EF state tracking gets messed up

                foreach (var attribute in newEnt.Attributes.Values) // todo: put in constant
                {
                    // find attribute definition
                    var attribDef = attributeDefs.SingleOrDefault(a => string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (attribDef == null)
                        throw new Exception($"trying to save attribute {attribute.Name} but can\'t find definition in DB");
                    if(attribDef.Type == AttributeTypeEnum.Entity.ToString()) continue;

                    foreach (var value in attribute.Values)
                        dbEnt.ToSicEavValues.Add(new ToSicEavValues
                        {
                            AttributeId = attribDef.AttributeId,
                            Value = value.SerializableObject?.ToString() ?? "",
                            ChangeLogCreated = changeId, // todo: remove some time later
                            ToSicEavValuesDimensions = value.Languages?.Select(l => new ToSicEavValuesDimensions
                            {
                                DimensionId = zoneLanguages.Single(ol => ol.EnvironmentKey.ToLowerInvariant() == l.Key).DimensionId,
                                ReadOnly = l.ReadOnly
                            }).ToList()
                        });
                }
                DbContext.SqlDb.SaveChanges(); // save all the values we just added

                #endregion



                #region Step 5: Save / update all relationships

                DbContext.Relationships.SaveRelationships(newEnt, dbEnt, attributeDefs, so);

                #endregion



                #region Step 6: Ensure versioning
                DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, useDelayedSerialize: true);
                #endregion



                #region Workaround for preserving the last guid - temp

                TempLastSaveGuid = dbEnt.EntityGuid;
                #endregion

                //throw new Exception("test exception, don't want to persist till I'm sure it's pretty stable");
                // finish transaction - finalize
                ownTransaction?.Commit();
            }
            catch 
            {
                // if anything fails, undo everything
                ownTransaction?.Rollback();
                throw;
            }

            return dbEnt.EntityId;
        }

        public Guid TempLastSaveGuid;




        public List<int> SaveEntity(List<IEntity> entities, SaveOptions saveOptions)
        {
            var ids = new List<int>();
            var ownTransaction = DbContext.SqlDb.Database.CurrentTransaction == null ? DbContext.SqlDb.Database.BeginTransaction() : null;

            try
            {
                DbContext.Versioning.QueueDuringAction(() =>
                {
                    foreach (var entity in entities)
                        DbContext.DoAndSave(() => ids.Add(SaveEntity(entity, saveOptions)));

                    DbContext.Relationships.ImportRelationshipQueueAndSave();
                });
                ownTransaction?.Commit();
            }
            catch
            {
                ownTransaction?.Rollback();
                throw;
            }
            return ids;
        }

    }


}
