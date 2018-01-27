using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        public const int RepoIdForJsonEntities = 1;

        private List<DimensionDefinition> _zoneLanguages;

        private readonly JsonSerializer _jsonifier = new JsonSerializer();

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
                _zoneLanguages = so.Languages ?? throw new Exception("languages missing in save-options. cannot continue");

            if(usedLanguages.Count > 0)
                if (!usedLanguages.All(l => _zoneLanguages.Any(zl => zl.Matches(l.Key))))
                    throw new Exception("found languages in save which are not available in environment - used has " + usedLanguages.Count + " target has " + _zoneLanguages.Count + " used-list: '" + string.Join(",", usedLanguages.Select(l => l.Key).ToArray()) + "'");
            Log.Add($"lang checks - zone language⋮{_zoneLanguages.Count}, usedLanguages⋮{usedLanguages.Count}");
            Log.Add(() => $"langs zone:[{string.Join(",", _zoneLanguages.Select(z => z.EnvironmentKey))}] used:[{string.Join(",", usedLanguages.Select(u => u.Key))}]");
            
            #endregion Test languages exist

            // check if saving should be with db-type or with the plain json
            var saveJson = newEnt.Type.RepositoryType != RepositoryTypes.Sql;
            string jsonExport;
            Log.Add($"save json:{saveJson}");
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
                    newEnt.ResetEntityId(existingDraftId ?? 0);  // set to the draft OR 0 = new
                    hasAdditionalDraft = false; // not additional any more, as we're now pointing this as primary
                }
            }
            var isNew = newEnt.EntityId <= 0;   // remember how we want to work...

            var contentTypeId = saveJson 
                ? RepoIdForJsonEntities 
                : DbContext.AttribSet.GetIdWithEitherName(newEnt.Type.StaticName);
            var attributeDefs = saveJson
                ? null
                : DbContext.AttributesDefinition.GetAttributeDefinitions(contentTypeId).ToList();
            Log.Add($"header checked type:{contentTypeId}, attribDefs⋮{attributeDefs?.Count}");
            if (attributeDefs != null)
                Log.Add(() => $"attribs: [{string.Join(",", attributeDefs.Select(a => a.AttributeId + ":" + a.StaticName))}]");
            #endregion Step 2



            ToSicEavEntities dbEnt = null;

            var changeLogId = DbContext.Versioning.GetChangeLogId();

            DbContext.DoInTransaction(() =>
            {

                #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

                if (isNew)
                {
                    Log.Add("create new...");

                    dbEnt = CreateNewInDb(newEnt, changeLogId, contentTypeId);
                    // update the ID - for versioning and/or json persistence
                    newEnt.ResetEntityId(dbEnt.EntityId); // update this, as it was only just generated
                    jsonExport = _jsonifier.Serialize(newEnt);

                    if (saveJson)
                    {
                        dbEnt.Json = jsonExport ; // = _jsonifier.Serialize(newEnt);
                        dbEnt.ContentType = newEnt.Type.StaticName;
                        DbContext.SqlDb.SaveChanges();
                    }
                    Log.Add($"create new i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}");
                }
                else
                {
                    #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                    // new: always change the draft if there is one! - it will then either get published, or not...
                    dbEnt = DbContext.Entities.GetDbEntity(newEnt.EntityId); // get the published one (entityid is always the published id)

                    var stateChanged = dbEnt.IsPublished != newEnt.IsPublished;
                    Log.Add($"used existing i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}, newstate:{newEnt.IsPublished}, state-changed:{stateChanged}, has-additional-draft:{hasAdditionalDraft}");

                    #region If draft but should be published, correct what's necessary

                    // Update as Published but Current Entity is a Draft-Entity
                    // case 1: saved entity is a draft and save wants to publish
                    // case 2: new data is set to not publish, but we don't want a branch
                    if (stateChanged || hasAdditionalDraft)
                    {
                        // 2018-01-27a 2dm disabled this - believe it's not necessary any more
                        // if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                        //if (dbEnt.PublishedEntityId.HasValue)
                        //    DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, false);

                        // now reset the branch/entity-state to properly set the state / purge the draft
                        dbEnt = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt, existingDraftId,
                            newEnt.IsPublished);

                        // update ID of the save-entity, as it's used again later on...
                        newEnt.ResetEntityId(dbEnt.EntityId);
                    }

                    #endregion

                    // increase version
                    dbEnt.Version++;
                    (newEnt as Entity)?.SetVersion(dbEnt.Version);
                    
                    // prepare export for save json OR versioning later on
                    jsonExport = _jsonifier.Serialize(newEnt);

                    if (saveJson)
                    {
                        dbEnt.Json = jsonExport;// = _jsonifier.Serialize(newEnt);
                        dbEnt.AttributeSetId = contentTypeId;       // in case the previous entity wasn't json stored yet
                        dbEnt.ContentType = newEnt.Type.StaticName; // in case the previous entity wasn't json stored yet
                    }

                    // first, clean up all existing attributes / values (flush)
                    // this is necessary after remove, because otherwise EF state tracking gets messed up
                    DbContext.DoAndSave(() => dbEnt.ToSicEavValues.Clear());

                    #endregion Step 3b
                }

                #endregion Step 3

                #region Step 4: Save all normal attributes / values & relationships

                if (!saveJson)
                {
                    // save all the values we just added
                    DbContext.DoAndSave(() =>
                        SaveAttributesInDbModel(newEnt, so, attributeDefs, dbEnt, changeLogId)
                    );

                    DbContext.Relationships.SaveRelationships(newEnt, dbEnt, attributeDefs, so);
                }
                else if (isNew)
                    Log.Add("won't save properties / relationships in db model as it's json");
                else 
                {
                    // in update scenarios, the old data could have been a db-model, so clear that
                    ClearAttributesInDbModel(newEnt.EntityId);
                    DbContext.Relationships.FlushChildrenRelationships(new List<int> {newEnt.EntityId});
                }

                #endregion



                #region Step 6: Ensure versioning

                if(jsonExport == null)
                    throw new Exception("trying to save version history entry, but jsonexport isn't ready");
                //if (saveJson)
                    DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, jsonExport);
                //else
                //    DbContext.Versioning.SaveEntity(dbEnt.EntityId, dbEnt.EntityGuid, useDelayedSerialize: true);

                #endregion



                #region Workaround for preserving the last guid (temp - improve some day...)

                TempLastSaveGuid = dbEnt.EntityGuid;

                #endregion

            });

            Log.Add("save done for id:" + dbEnt?.EntityId);
            return dbEnt.EntityId;
        }

        private ToSicEavEntities CreateNewInDb(IEntity newEnt, int changeId, int contentTypeId)
        {
            var dbEnt = new ToSicEavEntities
            {
                AppId = DbContext.AppId,
                AssignmentObjectTypeId = newEnt.MetadataFor?.TargetType ?? Constants.NotMetadata,
                KeyNumber = newEnt.MetadataFor?.KeyNumber,
                KeyGuid = newEnt.MetadataFor?.KeyGuid,
                KeyString = newEnt.MetadataFor?.KeyString,
                ChangeLogCreated = changeId,
                ChangeLogModified = changeId,
                EntityGuid = newEnt.EntityGuid != Guid.Empty ? newEnt.EntityGuid : Guid.NewGuid(),
                IsPublished = newEnt.IsPublished,
                PublishedEntityId = newEnt.IsPublished ? null : ((Entity) newEnt).GetPublishedIdForSaving(),
                Owner = DbContext.UserName,
                AttributeSetId = contentTypeId,
                Version = 1,
                Json = null // use null, as we must wait to serialize till we have the entityId
            };

            DbContext.SqlDb.Add(dbEnt);
            DbContext.SqlDb.SaveChanges();

            return dbEnt;
        }

        /// <summary>
        /// Remove values and attached dimensions of these values from the DB
        /// Important when updating json-entities, to ensure we don't keep trash around
        /// </summary>
        /// <param name="entityId"></param>
        private void ClearAttributesInDbModel(int entityId)
        {
            var val = DbContext.SqlDb.ToSicEavValues
                .Include(v => v.ToSicEavValuesDimensions)
                .Where(v => v.EntityId == entityId)
                .ToList();

            if (val.Count == 0) return;

            var dims = val.SelectMany(v => v.ToSicEavValuesDimensions);
            DbContext.DoAndSave(() => DbContext.SqlDb.RemoveRange(dims));
            DbContext.DoAndSave(() => DbContext.SqlDb.RemoveRange(val));
        }

        private void SaveAttributesInDbModel(IEntity newEnt, 
            SaveOptions so, 
            List<ToSicEavAttributes> attributeDefs, 
            ToSicEavEntities dbEnt,
            int changeId)
        {
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
                        toSicEavValuesDimensions = value.Languages?.Select(l => new ToSicEavValuesDimensions
                        {
                            DimensionId = _zoneLanguages.Single(ol => ol.Matches(l.Key)).DimensionId,
                            ReadOnly = l.ReadOnly
                        }).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "something went wrong building the languages to save - " +
                            "your DB probably has some wrong language information which doesn't match; " +
                            "maybe even a duplicate entry for a language code" +
                            " - see https://github.com/2sic/2sxc/issues/1293",
                            ex);
                    }

                    #endregion

                    Log.Add(() =>
                        $"add attrib:{attribDef.AttributeId}/{attribDef.StaticName} vals⋮{attribute.Values?.Count}, dim⋮{toSicEavValuesDimensions?.Count}");

                    dbEnt.ToSicEavValues.Add(new ToSicEavValues
                    {
                        AttributeId = attribDef.AttributeId,
                        Value = value.Serialized ?? "",
                        ChangeLogCreated = changeId, // todo: remove some time later
                        ToSicEavValuesDimensions = toSicEavValuesDimensions
                    });
                }
            }
        }


        /// <summary>
        /// Temp helper to provide the last guid to the caller
        /// this is a messy workaround, must find a better way someday...
        /// </summary>
        public Guid TempLastSaveGuid;




        /// <summary>
        /// Save a list of entities in one large go
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        internal List<int> SaveEntity(List<IEntity> entities, SaveOptions saveOptions)
        {
            Log.Add($"save many count:{entities?.Count}");
            var ids = new List<int>();

            DbContext.DoInTransaction(()
                => DbContext.Versioning.QueueDuringAction(()
                    =>
                {
                    entities?.ForEach(e => DbContext.DoAndSave(() => ids.Add(SaveEntity(e, saveOptions))));
                    DbContext.Relationships.ImportRelationshipQueueAndSave();
                }));
            return ids;
        }

    }


}
