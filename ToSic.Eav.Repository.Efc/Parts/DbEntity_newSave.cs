using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        public bool DebugKeepTransactionOpen = false;
        public IDbContextTransaction DebugTransaction; 

        public int SaveEntity(IEntity eToSave, SaveOptions so)
        {

            #region Step 1: Do some initial error checking and preparations
            if (eToSave == null)
                throw new ArgumentNullException(nameof(eToSave));

            if (eToSave.Type == null)
                throw new Exception("trying to save entity without known content-type, cannot continue");

            #region Test what languages are given, and check if they exist in the target system
            var usedLanguages = eToSave.GetUsedLanguages();

            var zoneLanguages = DbContext.Dimensions.GetLanguages();

            if(usedLanguages.Count > 0)
                if (!usedLanguages.All(l => zoneLanguages.Any(zl => zl.ExternalKey.ToLowerInvariant() == l.Key)))
                    throw new Exception("found languages in save which are not available in environment");
            #endregion Test languages exist

            #endregion Step 1


            #region Step 2: check header record - does it already exist, what ID should we use, etc.

            var isNew = eToSave.EntityId <= 0;// eToSave.RepositoryId <= 0;
            var dbId = eToSave.RepositoryId > 0 ? eToSave.RepositoryId : eToSave.EntityId;

            var contentTypeId = DbContext.AttribSet.GetAttributeSetIdWithEitherName(eToSave.Type.StaticName);
            var attributeDefs = DbContext.AttributesDefinition.GetAttributeDefinitions(contentTypeId).ToList();

            #endregion Step 2

            var transaction = DbContext.SqlDb.Database.BeginTransaction();
            var changeId = DbContext.Versioning.GetChangeLogId();
            ToSicEavEntities dbEntity = null;
            try
            {

                #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

                if (isNew)
                {
                    #region Step 3a: Create new

                    dbEntity = new ToSicEavEntities
                    {
                        AssignmentObjectTypeId = eToSave.Metadata?.TargetType ?? Constants.NotMetadata,
                        KeyNumber = eToSave.Metadata?.KeyNumber,
                        KeyGuid = eToSave.Metadata?.KeyGuid,
                        KeyString = eToSave.Metadata?.KeyString,
                        ChangeLogCreated = changeId,
                        ChangeLogModified = changeId,
                        EntityGuid = eToSave.EntityGuid != Guid.Empty ? eToSave.EntityGuid : Guid.NewGuid(),
                        IsPublished = eToSave.IsPublished,
                        PublishedEntityId = eToSave.IsPublished ? null : ((Entity)eToSave).GetPublishedIdForSaving(),
                        Owner = DbContext.UserName,
                        AttributeSetId = contentTypeId
                    };

                    DbContext.SqlDb.Add(dbEntity);
                    DbContext.SqlDb.SaveChanges();
                    #endregion
                }
                else
                {
                    #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                    // todo: check if repo-id is always there, may need to use repoid OR entityId

                    dbEntity = DbContext.Entities.GetDbEntity(dbId);
                    var existingDraftId = DbContext.Publishing.GetDraftEntityId(eToSave.EntityId);

                    #region Unpublished Save (Draft-Saves) - do some possible error checking

                    // Current Entity is published but Update as a draft
                    if (dbEntity.IsPublished && !eToSave.IsPublished && !so.ForceNoBranche)
                        // Prevent duplicate Draft
                        throw existingDraftId.HasValue
                            ? new InvalidOperationException(
                                $"Published EntityId {dbId} has already a draft with EntityId {existingDraftId}")
                            : new InvalidOperationException(
                                "It seems you're trying to update a published entity with a draft - this is not possible - the save should actually try to create a new draft instead without calling update.");

                    // Prevent editing of Published if there's a draft
                    if (dbEntity.IsPublished && existingDraftId.HasValue)
                        throw new Exception(
                            $"Update Entity not allowed because a draft exists with EntityId {existingDraftId}");

                    #endregion

                    #region If draft but should be published, correct what's necessary

                    // Update as Published but Current Entity is a Draft-Entity
                    // case 1: saved entity is a draft and save wants to publish
                    // case 2: new data is set to not publish, but we don't want a branch
                    if (!dbEntity.IsPublished && eToSave.IsPublished || !eToSave.IsPublished && so.ForceNoBranche)
                    {
                        if (dbEntity.PublishedEntityId.HasValue)
                            // if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                            DbContext.Versioning.SaveEntity(dbEntity.EntityId, dbEntity.EntityGuid, false);
                        dbEntity = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(eToSave.RepositoryId,
                            eToSave.IsPublished); // must save intermediate because otherwise we get duplicate IDs
                    }

                    #endregion

                    #endregion Step 3b
                }

                #endregion Step 3

                #region Step 4: Save all normal values
                // first, clean up all existing attributes / values (flush)
                dbEntity.ToSicEavValues.Clear();
                DbContext.SqlDb.SaveChanges();  // this is necessary after remove, because otherwise EF state tracking gets messed up

                foreach (var attribute in eToSave.Attributes.Values) // todo: put in constant
                {
                    // find attribute definition
                    var attribDef = attributeDefs.SingleOrDefault(a => string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (attribDef == null)
                        throw new Exception($"trying to save attribute {attribute.Name} but can\'t find definition in DB");
                    if(attribDef.Type == AttributeTypeEnum.Entity.ToString()) continue;

                    foreach (var value in attribute.Values)
                        dbEntity.ToSicEavValues.Add(new ToSicEavValues
                        {
                            AttributeId = attribDef.AttributeId,
                            Value = value.SerializableObject.ToString(),
                            ChangeLogCreated = changeId, // todo: remove some time later
                            ToSicEavValuesDimensions = value.Languages?.Select(l => new ToSicEavValuesDimensions
                            {
                                DimensionId = zoneLanguages.Single(ol => ol.ExternalKey.ToLowerInvariant() == l.Key).DimensionId,
                                ReadOnly = l.ReadOnly
                            }).ToList()
                        });
                }
                DbContext.SqlDb.SaveChanges(); // save all the values we just added

                #endregion

                #region Step 5: Save / update all relationships

                DbContext.Relationships.SaveRelationships(eToSave, dbEntity, attributeDefs, so);

                #endregion

                #region Ensure versioning
                DbContext.Versioning.SaveEntity(dbEntity.EntityId, dbEntity.EntityGuid, useDelayedSerialize: true);
                #endregion

                #region Workaround for preserving the last guid - temp

                TempLastSaveGuid = dbEntity.EntityGuid;
                #endregion

                //throw new Exception("test exception, don't want to persist till I'm sure it's pretty stable");
                // finish transaction - finalize
                if (DebugKeepTransactionOpen)
                    DebugTransaction = transaction;
                else
                    transaction.Commit();
            }
            catch 
            {
                // if anything fails, undo everything
                transaction.Rollback();
            }

            return dbEntity.EntityId;
        }

        public Guid TempLastSaveGuid;

    }
}
