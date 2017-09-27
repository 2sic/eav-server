﻿using System;
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
        private List<DimensionDefinition> _zoneLanguages;

        internal int SaveEntity(IEntity newEnt, SaveOptions so)
        {
            Log.Add($"save start for id:{newEnt?.EntityId}/{newEnt?.EntityGuid}");
            #region Step 1: Do some initial error checking and preparations
            if (newEnt == null)
                throw new ArgumentNullException(nameof(newEnt));

            if (newEnt.Type == null)
                throw new Exception("trying to save entity without known content-type, cannot continue");

            #region Test what languages are given, and check if they exist in the target system
            var usedLanguages = newEnt.GetUsedLanguages();

            // continue here - must ensure that the languages are passed in, cached - or are cached on the DbEntity... for multiple saves
            if (_zoneLanguages == null)
                _zoneLanguages = so.Languages;
            if (_zoneLanguages == null)
                throw new Exception("languages missing in save-options. cannot continue");

            if(usedLanguages.Count > 0)
                if (!usedLanguages.All(l => _zoneLanguages.Any(zl => zl.Matches(l.Key))))
                    throw new Exception("found languages in save which are not available in environment - used has " + usedLanguages.Count + " target has " + _zoneLanguages.Count + " used-list: '" + string.Join(",", usedLanguages.Select(l => l.Key).ToArray()) + "'");
            Log.Add($"lang checks - zone language⋮{_zoneLanguages.Count}, usedLanguages⋮{usedLanguages.Count}");
            Log.Add(() => $"langs zone:[{string.Join(",", _zoneLanguages.Select(z => z.EnvironmentKey))}] used:[{string.Join(",", usedLanguages.Select(u => u.Key))}]");
            #endregion Test languages exist

            #endregion Step 1



            #region Step 2: check header record - does it already exist, what ID should we use, etc.

            // If we think we'll update an existing entity...
            // ...we have to check if we'll actualy update the draft of the entity
            // ...or create a new draft (branch)
            int? existingDraftId = null;
            var hasAdditionalDraft = false;
            if (newEnt.EntityId > 0)
            {
                existingDraftId = DbContext.Publishing.GetDraftBranchEntityId(newEnt.EntityId);  // find a draft of this - note that it won't find anything, if the item itself is the draft
                hasAdditionalDraft = existingDraftId != null && existingDraftId.Value != newEnt.EntityId;  // only true, if there is an "attached" draft; false if the item itself is draft
                Log.Add($"draft check: existing:{existingDraftId}, hasAdd:{hasAdditionalDraft}");
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
            Log.Add($"header checked type:{contentTypeId}, attribDefs⋮{attributeDefs.Count}");
            Log.Add(() => $"attribs: [{string.Join(",", attributeDefs.Select(a => a.AttributeId + ":" + a.StaticName))}]");
            #endregion Step 2



            ToSicEavEntities dbEnt = null;

            var changeId = DbContext.Versioning.GetChangeLogId();
            DbContext.DoInTransaction(() =>
            {

                #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

                if (isNew)
                {
                    #region Step 3a: Create new

                    Log.Add("create new...");
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
                        PublishedEntityId = newEnt.IsPublished ? null : ((Entity) newEnt).GetPublishedIdForSaving(),
                        Owner = DbContext.UserName,
                        AttributeSetId = contentTypeId,
                        Version = 1
                    };

                    DbContext.SqlDb.Add(dbEnt);
                    DbContext.SqlDb.SaveChanges();
                    Log.Add($"create new i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}");

                    #endregion
                }
                else
                {
                    #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                    // todo: check if repo-id is always there, may need to use repoid OR entityId
                    // new: always change the draft if there is one! - it will then either get published, or not...
                    dbEnt = DbContext.Entities.GetDbEntity(newEnt.EntityId);

                    var stateChanged = dbEnt.IsPublished != newEnt.IsPublished;
                    Log.Add($"used existing i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}, newstate:{newEnt.IsPublished}, state-changed:{stateChanged}, has-additional-draft:{hasAdditionalDraft}");

                    #region If draft but should be published, correct what's necessary

                    // Update as Published but Current Entity is a Draft-Entity
                    // case 1: saved entity is a draft and save wants to publish
                    // case 2: new data is set to not publish, but we don't want a branch
                    if (stateChanged || hasAdditionalDraft)
                    {
                        // if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                        if (dbEnt.PublishedEntityId.HasValue)
                            DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, false);

                        // now reset the branch/entity-state to properly set the state / purge the draft
                        dbEnt = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt, existingDraftId,
                            newEnt.IsPublished);

                        ((Entity) newEnt).ChangeIdForSaving(dbEnt.EntityId);
                            // update ID of the save-entity, as it's used again later on...
                    }

                    #endregion

                    // increase version
                    dbEnt.Version++;

                    // first, clean up all existing attributes / values (flush)
                    // this is necessary after remove, because otherwise EF state tracking gets messed up
                    DbContext.DoAndSave(() => dbEnt.ToSicEavValues.Clear());

                    #endregion Step 3b
                }

                #endregion Step 3

                #region Step 4: Save all normal values


                foreach (var attribute in newEnt.Attributes.Values)
                {
                    Log.Add($"add attrib:{attribute.Name}");
                    // find attribute definition
                    var attribDef =
                        attributeDefs.SingleOrDefault(
                            a => string.Equals(a.StaticName, attribute.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (attribDef == null)
                    {
                        if (!so.DiscardattributesNotInType)
                            throw new Exception(
                                $"trying to save attribute {attribute.Name} but can\'t find definition in DB");
                        Log.Add("attribute not found, will skip according to save-options");
                        continue;
                    }
                    if (attribDef.Type == AttributeTypeEnum.Entity.ToString())
                    {
                        Log.Add("type is entity, skip for now as relationships are processed later");
                        continue;
                    }

                    foreach (var value in attribute.Values)
                    {
                        #region prepare languages - has extensive error reporting, to help in case any db-data is bad
                        List<ToSicEavValuesDimensions> toSicEavValuesDimensions;
                        try
                        {
                            toSicEavValuesDimensions = value.Languages?.Select(l => new ToSicEavValuesDimensions {
                                DimensionId = _zoneLanguages.Single(ol => ol.Matches(l.Key)).DimensionId,
                                ReadOnly = l.ReadOnly
                            }).ToList();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("something went wrong building the languages to save - your DB probably has some wrong language information which doesn't match; maybe even a duplicate entry for a language code - see https://github.com/2sic/2sxc/issues/1293", ex);
                        }
                        #endregion
                        Log.Add(() => $"add attrib:{attribDef.AttributeId}/{attribDef.StaticName} vals⋮{attribute.Values.Count}, dim⋮{toSicEavValuesDimensions?.Count}");

                        dbEnt.ToSicEavValues.Add(new ToSicEavValues
                        {
                            AttributeId = attribDef.AttributeId,
                            Value = value.Serialized ?? "",
                            ChangeLogCreated = changeId, // todo: remove some time later
                            ToSicEavValuesDimensions = toSicEavValuesDimensions
                        });

                    }
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
            });

            Log.Add("save done for id:" + dbEnt?.EntityId);
            return dbEnt.EntityId;
        }

        public Guid TempLastSaveGuid;




        internal List<int> SaveEntity(List<IEntity> entities, SaveOptions saveOptions)
        {
            Log.Add($"save many count:{entities?.Count}");
            var ids = new List<int>();

            DbContext.DoInTransaction(() => DbContext.Versioning.QueueDuringAction(() =>
            {
                foreach (var entity in entities)
                    DbContext.DoAndSave(() => ids.Add(SaveEntity(entity, saveOptions)));

                DbContext.Relationships.ImportRelationshipQueueAndSave();
            }));
            return ids;
        }

    }


}
