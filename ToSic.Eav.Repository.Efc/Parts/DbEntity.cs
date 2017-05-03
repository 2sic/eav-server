using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Logging;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Persistence.Efc.Models;
using System.Linq.Expressions;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbEntity: BllCommandBase
    {
        public DbEntity(DbDataController cntx) : base(cntx)
        {
        }



        #region Get Commands

        private IQueryable<ToSicEavEntities> EntityQuery 
            => DbContext.SqlDb.ToSicEavEntities
                   .Include(e => e.RelationshipsWithThisAsParent)
                   .Include(e => e.RelationshipsWithThisAsChild);

        private IQueryable<ToSicEavEntities> IncludeMultiple(IQueryable<ToSicEavEntities> origQuery, string additionalTables)//Expression<Func<ToSicEavEntities, IQueryable<ToSicEavEntities>>> navigationPropertyPath)
        {
            //var origQuery = EntityQuery;
            additionalTables.Split(',').ToList().ForEach(a => origQuery = origQuery.Include(a.Trim()));
            return origQuery;
        }

        /// <summary>
        /// Get a single Entity by EntityId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        internal ToSicEavEntities GetDbEntity(int entityId)
            =>  //DbContext.SqlDb.ToSicEavEntities
                //.Include(e => e.RelationshipsWithThisAsParent)
                //.Include(e => e.RelationshipsWithThisAsChild)
                EntityQuery.Single(e => e.EntityId == entityId);

        internal ToSicEavEntities GetDbEntity(int entityId, string includes)
            => IncludeMultiple(EntityQuery, includes).Single(e => e.EntityId == entityId);

        /// <summary>
        /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public ToSicEavEntities GetMostCurrentDbEntity(Guid entityGuid)
            // GetEntity should never return a draft entity that has a published version
            => GetEntitiesByGuid(entityGuid).Single(e => !e.PublishedEntityId.HasValue);
        


        internal IQueryable<ToSicEavEntities> GetEntitiesByGuid(Guid entityGuid) 
            => EntityQuery // DbContext.SqlDb.ToSicEavEntities
            //.Include(e => e.RelationshipsWithThisAsParent)
            //.Include(e => e.RelationshipsWithThisAsChild)
            .Where(e => e.EntityGuid == entityGuid && !e.ChangeLogDeleted.HasValue &&
                !e.AttributeSet.ChangeLogDeleted.HasValue && e.AttributeSet.AppId == DbContext.AppId);


        internal IQueryable<ToSicEavEntities> GetEntitiesByType(ToSicEavAttributeSets set)
        => EntityQuery // DbContext.SqlDb.ToSicEavEntities
                //.Include(e => e.RelationshipsWithThisAsParent)
                //.Include(e => e.RelationshipsWithThisAsChild)
                .Include(e => e.ToSicEavValues)
                .Where(e => e.AttributeSet == set);


        /// <summary>
        /// Test whether Entity exists on current App and is not deleted
        /// </summary>
        internal bool EntityExists(Guid entityGuid) => GetEntitiesByGuid(entityGuid).Any();

        
        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
        /// </summary>
        internal IQueryable<ToSicEavEntities> GetAssignedEntities(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null, string includes = null)
        {
            var origQuery = DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.AssignmentObjectTypeId == assignmentObjectTypeId
                   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
                   && e.ChangeLogDeleted == null);
            if (!string.IsNullOrEmpty(includes))
                origQuery = IncludeMultiple(origQuery, includes);
            return origQuery;
        }

        /// <summary>
        /// Get a Metadata items which enhance existing Entities, 
        /// and use the GUID to keep reference. This is extra complex, because the Guid can be in use multiple times on various apps
        /// </summary>
        internal IQueryable<ToSicEavEntities> GetEntityMetadataByGuid(int appId, Guid keyGuid, string includes = null)
        {
            int assignmentObjectTypeId = Constants.MetadataForEntity;
            var query = GetAssignedEntities(Constants.MetadataForEntity, keyGuid: keyGuid, includes: includes)
            //var origQuery = DbContext.SqlDb.ToSicEavEntities
                .Where(e => e.AttributeSet.AppId == appId);
            //    .Where(e => e.AssignmentObjectTypeId == assignmentObjectTypeId
            //       && e.KeyGuid == keyGuid
            //       && e.ChangeLogDeleted == null);
            //if (!string.IsNullOrEmpty(includes))
            //    origQuery = IncludeMultiple(origQuery, includes);
            return query;
        }

        #endregion

        #region Add Commands
        /// <summary>
        /// Import a new Entity
        /// </summary>
        internal ToSicEavEntities AddImportEntity(int attributeSetId, ImpEntity impEntity, List<ImportLogItem> importLog, bool isPublished, int? publishedTarget)
        {
            return AddEntity(attributeSetId, impEntity.Values, 
                impEntity.KeyNumber, impEntity.KeyGuid, impEntity.KeyString, impEntity.KeyTypeId, 
                0, impEntity.EntityGuid, null, importLog, isPublished, publishedTarget);
        }
        
        
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public ToSicEavEntities AddEntity(int attributeSetId, IDictionary values, 
            int? keyNumber = null, Guid? keyGuid = null, string keyString = null, int keyTypeId = Constants.NotMetadata, 
            int sortOrder = 0, 
            Guid? entityGuid = null, ICollection<int> dimensionIds = null, List<ImportLogItem> updateLog = null, 
            bool isPublished = true, int? publishedEntityId = null)
        {
            // var skipCreate = false;
            var existingEntityId = 0;
            // Prevent duplicate add of FieldProperties
            if (keyTypeId == Constants.MetadataForField && keyNumber.HasValue)
            {
                var foundThisMetadata = GetAssignedEntities(Constants.MetadataForField, keyNumber.Value)
                        .FirstOrDefault(e => e.AttributeSetId == attributeSetId);
                if (foundThisMetadata != null)
                    existingEntityId = foundThisMetadata.EntityId;
            }

            var changeId = DbContext.Versioning.GetChangeLogId();

            #region generate brand-new entity if non exists yet
            if (existingEntityId == 0)
            {
                var newEntity = new ToSicEavEntities
                {
                    ConfigurationSet = null, // configurationSet,
                    AssignmentObjectTypeId = keyTypeId,
                    KeyNumber = keyNumber,
                    KeyGuid = keyGuid,
                    KeyString = keyString,
                    SortOrder = sortOrder,
                    ChangeLogCreated = changeId,
                    ChangeLogModified = changeId,
                    EntityGuid =
                        (entityGuid.HasValue && entityGuid.Value != new Guid()) ? entityGuid.Value : Guid.NewGuid(),
                    IsPublished = isPublished,
                    PublishedEntityId = isPublished ? null : publishedEntityId,
                    Owner = DbContext.UserName,
                    AttributeSetId = attributeSetId
                };

                DbContext.SqlDb.Add(newEntity);
                DbContext.SqlDb.SaveChanges();
                existingEntityId = newEntity.EntityId;
            }
            #endregion

            return SaveEntity(existingEntityId, values, /*masterRecord: true,*/ dimensionIds: dimensionIds, autoSave: false, updateLog: updateLog, isPublished: isPublished);

            //DbContext.SqlDb.SaveChanges();
            //DbContext.Versioning.SaveEntity(updatedEntity.EntityId, updatedEntity.EntityGuid, true);
            //return updatedEntity;
        }

        #endregion

        #region Clone
        /// <summary>
        /// Clone an Entity with all Values
        /// </summary>
        internal ToSicEavEntities CloneEntity(ToSicEavEntities sourceEntity, bool assignNewEntityGuid = false)
        {
            // var clone = sourceEntity; // 2017-04-19 todo validate //  DbContext.DbS.CopyEfEntity(sourceEntity);
            var versioningId = DbContext.Versioning.GetChangeLogId();
            var clone = new ToSicEavEntities()
            {
                AttributeSet = sourceEntity.AttributeSet,
                ConfigurationSet = sourceEntity.ConfigurationSet,
                AssignmentObjectTypeId = sourceEntity.AssignmentObjectTypeId,
                KeyGuid = sourceEntity.KeyGuid,
                KeyNumber = sourceEntity.KeyNumber,
                KeyString = sourceEntity.KeyString,
                ChangeLogCreated = versioningId,
                ChangeLogModified = versioningId
        };

            DbContext.SqlDb.Add(clone);

            DbContext.Values.CloneEntityValues(sourceEntity, clone);

            if (assignNewEntityGuid)
                clone.EntityGuid = Guid.NewGuid();

            return clone;
        }
        #endregion  

        #region Update
        /// <summary>
        /// Update an Entity
        /// </summary>
        /// <param name="repositoryId">EntityId as in the repository (so the draft would have that id)</param>
        /// <param name="newValues">new Values of this Entity</param>
        /// <param name="autoSave">auto save Changes to DB</param>
        /// <param name="dimensionIds">DimensionIds for all Values</param>
        /// <param name="masterRecord">Is this the Master Record/Language</param>
        /// <param name="updateLog">Update/Import Log List</param>
        /// <param name="preserveUndefinedValues">Preserve Values if Attribute is not specifeied in NewValues</param>
        /// <param name="isPublished">Is this Entity Published or a draft</param>
        /// <param name="forceNoBranch">this forces the published-state to be applied to the original, without creating a draft-branhc</param>
        /// <returns>the updated Entity</returns>
        public ToSicEavEntities SaveEntity(int repositoryId, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, /*bool masterRecord = true,*/ List<ImportLogItem> updateLog = null, bool preserveUndefinedValues = true, bool isPublished = true, bool forceNoBranch = false)
        {
            var entity = DbContext.SqlDb.ToSicEavEntities.Single(e => e.EntityId == repositoryId);
            var draftEntityId = DbContext.Publishing.GetDraftEntityId(repositoryId);

            #region Unpublished Save (Draft-Saves)
            // Current Entity is published but Update as a draft
            if (entity.IsPublished && !isPublished && !forceNoBranch)
                // Prevent duplicate Draft
                throw draftEntityId.HasValue
                    ? new InvalidOperationException($"Published EntityId {repositoryId} has already a draft with EntityId {draftEntityId}")
                    : new InvalidOperationException("It seems you're trying to update a published entity with a draft - this is not possible - the save should actually try to create a new draft instead without calling update.");

            // Prevent editing of Published if there's a draft
            if (entity.IsPublished && draftEntityId.HasValue)
                throw new Exception($"Update Entity not allowed because a draft exists with EntityId {draftEntityId}");

            #endregion

            #region If draft but should be published, correct what's necessary
            // Update as Published but Current Entity is a Draft-Entity
            // case 1: saved entity is a draft and save wants to publish
            // case 2: new data is set to not publish, but we don't want a branch
            if (!entity.IsPublished && isPublished || !isPublished && forceNoBranch)
            {
                if (entity.PublishedEntityId.HasValue)	// if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                    DbContext.Versioning.SaveEntity(entity.EntityId, entity.EntityGuid, false);
                entity = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(repositoryId, isPublished); // must save intermediate because otherwise we get duplicate IDs
            }
            #endregion


            if (dimensionIds == null)
                dimensionIds = new List<int>(0);

            // Load all Attributes and current Values - .ToList() to prevent (slow) lazy loading
            var attributes = DbContext.Attributes.GetAttributeDefinitions(entity.AttributeSetId).ToList();
            var dbValues = entity.EntityId != 0
                ? DbContext.SqlDb.ToSicEavValues
                    .Include(x => x.Attribute)
                    .Include(x => x.ToSicEavValuesDimensions)
                        .ThenInclude(d => d.Dimension)
                    .Where(v => v.EntityId == entity.EntityId && v.ChangeLogDeleted == null)
                    .ToList()
                : entity.ToSicEavValues.ToList();

            // Update Values from Import Model
            var newValuesImport = newValues as Dictionary<string, List<IImpValue>>;
            if (newValuesImport != null)
                UpdateEntityFromImportModel(entity, newValuesImport, updateLog, attributes, dbValues, preserveUndefinedValues);
            // Update Values from ValueViewModel
            else
                UpdateEntityDefault(entity, newValues, dimensionIds/*, masterRecord*/, attributes, dbValues);


            entity.ChangeLogModified = DbContext.Versioning.GetChangeLogId();

            DbContext.SqlDb.SaveChanges();	// must save now to generate EntityModel afterward for DataTimeline

            DbContext.Versioning.SaveEntity(entity.EntityId, entity.EntityGuid, useDelayedSerialize: true);

            return entity;
        }

        /// <summary>
        /// Update an Entity when using the Import
        /// </summary>
        private void UpdateEntityFromImportModel(ToSicEavEntities currentEntity, Dictionary<string, List<IImpValue>> newValuesImport, List<ImportLogItem> updateLog, List<ToSicEavAttributes> attributeList, List<ToSicEavValues> currentValues, bool keepAttributesMissingInImport)
        {
            if (updateLog == null)
                throw new ArgumentNullException(nameof(updateLog), "When Calling UpdateEntity() with newValues of Type IValueImportModel updateLog must be set.");

            // track updated values to remove values that were not updated automatically
            var updatedValueIds = new List<int>();
            var updatedAttributeIds = new List<int>();
            foreach (var newValue in newValuesImport)
            {
                #region Get Attribute Definition from List (or skip this field if not found)
                var attribute = attributeList.SingleOrDefault(a => a.StaticName == newValue.Key);
                if (attribute == null) // Attribute not found
                {
                    // Log Warning for all Values
                    updateLog.AddRange(newValue.Value.Select(v => new ImportLogItem(EventLogEntryType.Warning, "Attribute not found for Value")
                    {
                        ImpAttribute = new ImpAttribute { StaticName = newValue.Key },
                        ImpValue = v,
                        ImpEntity = v.ParentEntity
                    }));
                    continue;
                }
                #endregion

                updatedAttributeIds.Add(attribute.AttributeId);

                // Go through each value / dimensions combination
                foreach (var newSingleValue in newValue.Value)
                {
                    try
                    {
                        var updatedValue = DbContext.Values.UpdateValueByImport(currentEntity, attribute, currentValues, newSingleValue);

                        var updatedEavValue = updatedValue as ToSicEavValues;
                        if (updatedEavValue != null)
                            updatedValueIds.Add(updatedEavValue.ValueId);
                    }
                    catch (Exception ex)
                    {
                        updateLog.Add(new ImportLogItem(EventLogEntryType.Error, "Update Entity-Value failed")
                        {
                            ImpAttribute = new ImpAttribute { StaticName = newValue.Key },
                            ImpValue = newSingleValue,
                            ImpEntity = newSingleValue.ParentEntity,
                            Exception = ex
                        });
                    }
                }
            }

            // remove all existing values that were not updated
            // Logic should be:
            // Of all values - skip the ones we just modified and those which are deleted
            var untouchedValues = currentEntity.ToSicEavValues.Where(
                v => !updatedValueIds.Contains(v.ValueId) && v.ChangeLogDeleted == null);

            if (!keepAttributesMissingInImport)
            {
                untouchedValues.ToList().ForEach(v => v.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId());
            }
            else
            {
                // Since the importmodel contains all language/value combinations...
                // ...any "untouched" values should be removed, since all others were updated/touched.
                var untouchedValuesOfChangedAttributes = untouchedValues.Where(v => updatedAttributeIds.Contains(v.AttributeId));
                untouchedValuesOfChangedAttributes.ToList().ForEach(v => v.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId());
            }

        }





        /// <summary>
        /// Update an Entity when not using the Import
        /// </summary>
        private void UpdateEntityDefault(ToSicEavEntities entity, IDictionary newValues, ICollection<int> dimensionIds, /*bool masterRecord,*/ List<ToSicEavAttributes> attributes, List<ToSicEavValues> dbValues)
        {
            //var entityModel = entity.EntityId != 0 ? new Efc11Loader(DbContext.SqlDb).Entity(DbContext.AppId, entity.EntityId) : null;
            var newValuesTyped = DictionaryToValuesViewModel(newValues);
            foreach (var newValue in newValuesTyped)
            {
                var attribute = attributes.FirstOrDefault(a => a.StaticName == newValue.Key);
                if(attribute != null)
                    DbContext.Values.UpdateValue(entity, attribute, /*masterRecord,*/ dbValues, /*entityModel,*/ newValue.Value, dimensionIds);
            }

            #region if Dimensions are specified, purge/remove specified dimensions for Values that are not in newValues
            if (dimensionIds.Count > 0)
            {
                var attributeMetadataSource = DataSource.GetMetaDataSource(DbContext.ZoneId, DbContext.AppId);

                var keys = newValuesTyped.Keys.ToArray();
                // Get all Values that are not present in newValues
                var valuesToPurge = entity.ToSicEavValues.Where(v => !v.ChangeLogDeleted.HasValue && !keys.Contains(v.Attribute.StaticName) && v.ToSicEavValuesDimensions.Any(d => dimensionIds.Contains(d.DimensionId)));
                foreach (var valueToPurge in valuesToPurge)
                {
                    // Don't touch Value if attribute is not visible in UI
                    var attributeMetadata = attributeMetadataSource.GetAssignedEntities(Constants.MetadataForField, valueToPurge.AttributeId, "@All").FirstOrDefault();
                    if (attributeMetadata != null)
                    {
                        var visibleInEditUi = ((Attribute<bool?>)attributeMetadata["VisibleInEditUI"]).TypedContents;
                        if (visibleInEditUi == false)
                            continue;
                    }

                    // Check if the Value is only used in this supplied dimension (carefull, dont' know what to do if we have multiple dimensions!, must define later)
                    // if yes, delete/invalidate the value
                    if (valueToPurge.ToSicEavValuesDimensions.Count == 1)
                        valueToPurge.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
                    // if now, remove dimension from Value
                    else
                    {
                        foreach (var valueDimension in valueToPurge.ToSicEavValuesDimensions.Where(d => dimensionIds.Contains(d.DimensionId)).ToList())
                            valueToPurge.ToSicEavValuesDimensions.Remove(valueDimension);
                    }
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Convert IOrderedDictionary to <see cref="Dictionary{String, ValueViewModel}" /> (for backward capability)
        /// </summary>
        private Dictionary<string, ImpValueInside> DictionaryToValuesViewModel(IDictionary newValues)
        {
            if (newValues is Dictionary<string, ImpValueInside>)
                return (Dictionary<string, ImpValueInside>)newValues;

            return newValues.Keys.Cast<object>().ToDictionary(key => key.ToString(), key => new ImpValueInside { ReadOnly = false, Value = newValues[key] });
        }

        #region Delete Commands

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(int repositoryId, bool forceRemoveFromParents = false) => DeleteEntity(GetDbEntity(repositoryId), removeFromParents: forceRemoveFromParents);

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(Guid entityGuid) => DeleteEntity(GetMostCurrentDbEntity(entityGuid));

        /// <summary>
        /// Delete an Entity
        /// </summary>
        internal bool DeleteEntity(ToSicEavEntities entity, bool autoSave = true, bool removeFromParents = false)
        {
            if (entity == null)
                return false;

            #region Delete Related Records (Values, Value-Dimensions, Relationships)
            // Delete all Value-Dimensions
            var valueDimensions = entity.ToSicEavValues.SelectMany(v => v.ToSicEavValuesDimensions).ToList();
            DbContext.SqlDb.RemoveRange(valueDimensions); // 2017-04-19 todo validate // valueDimensions.ForEach(DbContext.SqlDb.DeleteObject);
            // Delete all Values
            DbContext.SqlDb.RemoveRange(entity.ToSicEavValues.ToList()); // 2017-04-19 todo validate //entity.ToSicEavValues.ToList().ForEach(DbContext.SqlDb.DeleteObject);
            // Delete all Parent-Relationships
            entity.RelationshipsWithThisAsParent.Clear(); // 2017-04-19 todo validate // /*.EntityParentRelationships*/.ToList().ForEach(DbContext.SqlDb.DeleteObject);
            if (removeFromParents)
                entity.RelationshipsWithThisAsChild.Clear(); // 2017-04-19 todo validate //*.EntityChildRelationships*/.ToList().ForEach(DbContext.SqlDb.DeleteObject);
            #endregion

            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                entity.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
                // Also delete the Draft (if any)
                var draftEntityId = DbContext.Publishing.GetDraftEntityId(entity.EntityId);
                if (draftEntityId.HasValue)
                    DeleteEntity(draftEntityId.Value);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                // Delete all Child-Relationships
                entity.RelationshipsWithThisAsChild.Clear(); // 2017-04-19 todo validate // /*.EntityChildRelationships*/.ToList().ForEach(DbContext.SqlDb.DeleteObject);
                DbContext.SqlDb.Remove(entity);
            }

            if (autoSave)
                DbContext.SqlDb.SaveChanges();

            return true;
        }


        public Tuple<bool, string> CanDeleteEntity(int entityId)
        {
            var messages = new List<string>();
            var entity = GetDbEntity(entityId);
            //var entityModel = new Efc11Loader(DbContext.SqlDb).Entity(DbContext.AppId, entityId);
            //if (!entityModel.IsPublished && entityModel.GetPublished() == null)	// always allow Deleting Draft-Only Entity 

            if (!entity.IsPublished && entity.PublishedEntityId == null)	// always allow Deleting Draft-Only Entity 
                return new Tuple<bool, string>(true, null);

            #region check if there are relationships where this is a child
            var parents = DbContext.SqlDb.ToSicEavEntityRelationships
                .Where(r => r.ChildEntityId == entityId)
                .Select(r => new TempEntityAndTypeInfos { EntityId = r.ParentEntityId, TypeId = r.ParentEntity.AttributeSetId} )
                .ToList();
            if (parents.Any())
            {
                TryToGetMoreInfosAboutDependencies(parents, messages);
                messages.Add($"found {parents.Count()} relationships where this is a child - the parents are: {string.Join(", ", parents)}.");
            }
            #endregion

            var entitiesAssignedToThis = GetAssignedEntities(Constants.MetadataForEntity, entityId)
                .Select(e => new TempEntityAndTypeInfos() { EntityId = e.EntityId, TypeId = e.AttributeSetId})
                .ToList();
            if (entitiesAssignedToThis.Any())
            {
                TryToGetMoreInfosAboutDependencies(entitiesAssignedToThis, messages);
                messages.Add($"found {entitiesAssignedToThis.Count()} entities which are metadata for this, assigned children (like in a pieline) or assigned for other reasons: {string.Join(", ", entitiesAssignedToThis)}.");
            }
            return Tuple.Create(!messages.Any(), string.Join(" ", messages));
        }

        private void TryToGetMoreInfosAboutDependencies(IEnumerable<TempEntityAndTypeInfos> dependencies, List<string> messages)
        {
            try
            {
                // try to get more infos about the parents
                foreach (var dependency in dependencies)
                    dependency.TypeName = DbContext.AttribSet.GetAttributeSet(dependency.TypeId).Name;
            }
            catch
            {
                messages.Add("Relationships but was not able to look up more details to show a nicer error.");
            }
            

        }

        private class TempEntityAndTypeInfos
        {
            internal int EntityId;
            internal int TypeId;
            internal string TypeName = "";

            public override string ToString() =>  EntityId + " (" + TypeName + ")";
            
        }

        #endregion

    }
}
