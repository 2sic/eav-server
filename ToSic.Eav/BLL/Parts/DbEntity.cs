using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.BLL.Parts
{
    public class DbEntity: BllCommandBase
    {
        public DbEntity(EavDataController cntx) : base(cntx)
        {
        }

        #region Get Commands

        /// <summary>
        /// Get a single Entity by EntityId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public Entity GetEntity(int entityId)
        {
            return Context.SqlDb.Entities.Single(e => e.EntityID == entityId);
        }

        /// <summary>
        /// Get a single Entity by EntityGuid. Ensure it's not deleted and has context's AppId
        /// </summary>
        /// <returns>Entity or throws InvalidOperationException</returns>
        public Entity GetEntity(Guid entityGuid)
        {
            // GetEntity should never return a draft entity that has a published version
            return GetEntitiesByGuid(entityGuid).Single(e => !e.PublishedEntityId.HasValue);
        }


        internal IQueryable<Entity> GetEntitiesByGuid(Guid entityGuid)
        {
            return
                Context.SqlDb.Entities.Where(
                    e =>
                        e.EntityGUID == entityGuid && !e.ChangeLogIDDeleted.HasValue &&
                        !e.Set.ChangeLogIDDeleted.HasValue && e.Set.AppID == Context.AppId);
        }

        /// <summary>
        /// Test whether Entity exists on current App and is not deleted
        /// </summary>
        public bool EntityExists(Guid entityGuid)
        {
            return GetEntitiesByGuid(entityGuid).Any();
        }


        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, int keyNumber)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, keyNumber);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, Guid keyGuid)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, null, keyGuid);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and Key.
        /// </summary>
        public IQueryable<Entity> GetEntities(int assignmentObjectTypeId, string keyString)
        {
            return GetEntitiesInternal(assignmentObjectTypeId, null, null, keyString);
        }

        /// <summary>
        /// Get a List of Entities with specified assignmentObjectTypeId and optional Key.
        /// </summary>
        internal IQueryable<Entity> GetEntitiesInternal(int assignmentObjectTypeId, int? keyNumber = null, Guid? keyGuid = null, string keyString = null)
        {
            return from e in Context.SqlDb.Entities
                   where e.AssignmentObjectTypeID == assignmentObjectTypeId
                   && (keyNumber.HasValue && e.KeyNumber == keyNumber.Value || keyGuid.HasValue && e.KeyGuid == keyGuid.Value || keyString != null && e.KeyString == keyString)
                   && e.ChangeLogIDDeleted == null
                   select e;
        }
        #endregion

        #region Add Commands
        /// <summary>
        /// Import a new Entity
        /// </summary>
        internal Entity AddEntity(int attributeSetId, Import.ImportEntity importEntity, List<ImportLogItem> importLog, bool isPublished, int? publishedTarget)
        {
            return AddEntity(null, attributeSetId, importEntity.Values, null, importEntity.KeyNumber, importEntity.KeyGuid, importEntity.KeyString, importEntity.AssignmentObjectTypeId, 0, importEntity.EntityGuid, null, updateLog: importLog, isPublished: isPublished, publishedEntityId: publishedTarget);
        }

        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(AttributeSet attributeSet, IDictionary values, int? configurationSet, int? key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(attributeSet, 0, values, configurationSet, key, null, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(int attributeSetId, IDictionary values, int? configurationSet, int? key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(null, attributeSetId, values, configurationSet, key, null, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(int attributeSetId, IDictionary values, int? configurationSet, Guid key, int assignmentObjectTypeId = Constants.DefaultAssignmentObjectTypeId, int sortOrder = 0, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true)
        {
            return AddEntity(null, attributeSetId, values, configurationSet, null, key, null, assignmentObjectTypeId, sortOrder, entityGuid, dimensionIds, isPublished: isPublished);
        }
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public Entity AddEntity(AttributeSet attributeSet, int attributeSetId, IDictionary values, int? configurationSet, int? keyNumber, Guid? keyGuid, string keyString, int assignmentObjectTypeId, int sortOrder, Guid? entityGuid, ICollection<int> dimensionIds, List<ImportLogItem> updateLog = null, bool isPublished = true, int? publishedEntityId = null)
        {
            // var skipCreate = false;
            var existingEntityId = 0;
            // Prevent duplicate add of FieldProperties
            if (assignmentObjectTypeId == Constants.AssignmentObjectTypeIdFieldProperties && keyNumber.HasValue)
            {
                var foundThisMetadata =
                    GetEntities(Constants.AssignmentObjectTypeIdFieldProperties, keyNumber.Value)
                        .FirstOrDefault(e => e.AttributeSetID == attributeSetId);
                if (foundThisMetadata != null)
                {
                    existingEntityId = foundThisMetadata.EntityID;
                    //skipCreate = true;

                    //throw new Exception(
                    //    string.Format("An Entity already exists with AssignmentObjectTypeId {0} and KeyNumber {1}",
                    //        Constants.AssignmentObjectTypeIdFieldProperties, keyNumber));
                }

            }

            var changeId = Context.Versioning.GetChangeLogId();

            if (existingEntityId == 0)
            {
                var newEntity = new Entity
                {
                    ConfigurationSet = configurationSet,
                    AssignmentObjectTypeID = assignmentObjectTypeId,
                    KeyNumber = keyNumber,
                    KeyGuid = keyGuid,
                    KeyString = keyString,
                    SortOrder = sortOrder,
                    ChangeLogIDCreated = changeId,
                    ChangeLogIDModified = changeId,
                    EntityGUID =
                        (entityGuid.HasValue && entityGuid.Value != new Guid()) ? entityGuid.Value : Guid.NewGuid(),
                    IsPublished = isPublished,
                    PublishedEntityId = isPublished ? null : publishedEntityId
                };

                if (attributeSet != null)
                    newEntity.Set = attributeSet;
                else
                    newEntity.AttributeSetID = attributeSetId;

                Context.SqlDb.AddToEntities(newEntity);

                Context.SqlDb.SaveChanges();
                existingEntityId = newEntity.EntityID;
            }

            var updatedEntity = UpdateEntity(existingEntityId, values, masterRecord: true, dimensionIds: dimensionIds, autoSave: false, updateLog: updateLog, isPublished: isPublished);

            Context.SqlDb.SaveChanges();

            return updatedEntity;
        }

        #endregion

        #region Clone
        /// <summary>
        /// Clone an Entity with all Values
        /// </summary>
        internal Entity CloneEntity(Entity sourceEntity, bool assignNewEntityGuid = false)
        {
            var clone = Context.DbS.CopyEfEntity(sourceEntity);

            Context.SqlDb.AddToEntities(clone);

            Context.Values.CloneEntityValues(sourceEntity, clone);

            if (assignNewEntityGuid)
                clone.EntityGUID = Guid.NewGuid();

            return clone;
        }
        #endregion  

        #region Update
        /// <summary>
        /// Update an Entity
        /// </summary>
        /// <param name="entityGuid">EntityGUID</param>
        /// <param name="newValues">new Values of this Entity</param>
        /// <param name="autoSave">auto save Changes to DB</param>
        /// <param name="dimensionIds">DimensionIds for all Values</param>
        /// <param name="masterRecord">Is this the Master Record/Language</param>
        /// <param name="updateLog">Update/Import Log List</param>
        /// <param name="preserveUndefinedValues">Preserve Values if Attribute is not specifeied in NewValues</param>
        /// <returns>the updated Entity</returns>
        public Entity UpdateEntity(Guid entityGuid, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, bool masterRecord = true, List<ImportLogItem> updateLog = null, bool preserveUndefinedValues = true)
        {
            var entity = GetEntity(entityGuid);
            return UpdateEntity(entity.EntityID, newValues, autoSave, dimensionIds, masterRecord, updateLog, preserveUndefinedValues);
        }


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
        /// <returns>the updated Entity</returns>
        public Entity UpdateEntity(int repositoryId, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, bool masterRecord = true, List<ImportLogItem> updateLog = null, bool preserveUndefinedValues = true, bool isPublished = true)
        {
            var entity = Context.SqlDb.Entities.Single(e => e.EntityID == repositoryId);
            var draftEntityId = Context.Publishing.GetDraftEntityId(repositoryId);

            #region Unpublished Save (Draft-Saves)
            // Current Entity is published but Update as a draft
            if (entity.IsPublished && !isPublished)
            {
                // Prevent duplicate Draft
                if (draftEntityId.HasValue)
                    throw new InvalidOperationException(string.Format("Published EntityId {0} has already a draft with EntityId {1}", repositoryId, draftEntityId));

                throw new InvalidOperationException("It seems you're trying to update a published entity with a draft - this is not possible - the save should actually try to create a new draft instead without calling update.");

                // create a new Draft-Entity
                entity = CloneEntity(entity);
                entity.IsPublished = false;
                entity.PublishedEntityId = repositoryId;
                
                // must save so we have a real entityId/repositoryId for later assignments
                Context.SqlDb.SaveChanges();
                entity = Context.SqlDb.Entities.Single(e => e.EntityID == Context.Publishing.GetDraftEntityId(repositoryId));
            }
            // Prevent editing of Published if there's a draft
            else if (entity.IsPublished && draftEntityId.HasValue)
            {
                throw new Exception(string.Format("Update Entity not allowed because a draft exists with EntityId {0}", draftEntityId));
            }
            #endregion

            #region If draft but should be published, correct what's necessary
            // Update as Published but Current Entity is a Draft-Entity
            if (!entity.IsPublished && isPublished)
            {
                if (entity.PublishedEntityId.HasValue)	// if Entity has a published Version, add an additional DateTimeline Item for the Update of this Draft-Entity
                    Context.Versioning.SaveEntityToDataTimeline(entity);
                entity = Context.Publishing.ClearDraftAndSetPublished(repositoryId); // must save intermediate because otherwise we get duplicate IDs
            }
            #endregion


            if (dimensionIds == null)
                dimensionIds = new List<int>(0);

            // Load all Attributes and current Values - .ToList() to prevent (slow) lazy loading
            var attributes = Context.Attributes.GetAttributes(entity.AttributeSetID).ToList();
            var currentValues = entity.EntityID != 0 ? Context.SqlDb.Values.Include("Attribute").Include("ValuesDimensions").Where(v => v.EntityID == entity.EntityID).ToList() : entity.Values.ToList();

            // Update Values from Import Model
            var newValuesImport = newValues as Dictionary<string, List<IValueImportModel>>;
            if (newValuesImport != null)
                UpdateEntityFromImportModel(entity, newValuesImport, updateLog, attributes, currentValues, preserveUndefinedValues);
            // Update Values from ValueViewModel
            else
                UpdateEntityDefault(entity, newValues, dimensionIds, masterRecord, attributes, currentValues);


            entity.ChangeLogIDModified = Context.Versioning.GetChangeLogId();

            Context.SqlDb.SaveChanges();	// must save now to generate EntityModel afterward for DataTimeline

            Context.Versioning.SaveEntityToDataTimeline(entity);

            return entity;
        }

        /// <summary>
        /// Update an Entity when using the Import
        /// </summary>
        internal void UpdateEntityFromImportModel(Entity currentEntity, Dictionary<string, List<IValueImportModel>> newValuesImport, List<ImportLogItem> updateLog, List<Attribute> attributeList, List<EavValue> currentValues, bool keepAttributesMissingInImport)
        {
            if (updateLog == null)
                throw new ArgumentNullException("updateLog", "When Calling UpdateEntity() with newValues of Type IValueImportModel updateLog must be set.");

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
                        ImportAttribute = new Import.ImportAttribute { StaticName = newValue.Key },
                        Value = v,
                        ImportEntity = v.ParentEntity
                    }));
                    continue;
                }
                #endregion

                updatedAttributeIds.Add(attribute.AttributeID);

                // Go through each value / dimensions combination
                foreach (var newSingleValue in newValue.Value)
                {
                    try
                    {
                        var updatedValue = Context.Values.UpdateValueByImport(currentEntity, attribute, currentValues, newSingleValue);

                        var updatedEavValue = updatedValue as EavValue;
                        if (updatedEavValue != null)
                            updatedValueIds.Add(updatedEavValue.ValueID);
                    }
                    catch (Exception ex)
                    {
                        updateLog.Add(new ImportLogItem(EventLogEntryType.Error, "Update Entity-Value failed")
                        {
                            ImportAttribute = new ImportAttribute { StaticName = newValue.Key },
                            Value = newSingleValue,
                            ImportEntity = newSingleValue.ParentEntity,
                            Exception = ex
                        });
                    }
                }
            }

            // remove all existing values that were not updated
            // Logic should be:
            // Of all values - skip the ones we just modified and those which are deleted
            var untouchedValues = currentEntity.Values.Where(
                v => !updatedValueIds.Contains(v.ValueID) && v.ChangeLogIDDeleted == null);

            if (!keepAttributesMissingInImport)
            {
                untouchedValues.ToList().ForEach(v => v.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId());
            }
            else
            {
                // Note 2015-10-20 2dm & 2rm
                // We changed this section a lot, and believe it now does what we expect. 
                // We believe that since the importmodel contains all language/value combinations...
                // ...so any "untouched" values should be removed, since all others were updated/touched.
                var untouchedValuesOfChangedAttributes = untouchedValues.Where(v => updatedAttributeIds.Contains(v.AttributeID));
                untouchedValuesOfChangedAttributes.ToList().ForEach(v => v.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId());
            }

        }





        /// <summary>
        /// Update an Entity when not using the Import
        /// </summary>
        internal void UpdateEntityDefault(Entity entity, IDictionary newValues, ICollection<int> dimensionIds, bool masterRecord, List<Attribute> attributes, List<EavValue> currentValues)
        {
            var entityModel = entity.EntityID != 0 ? new DbLoadIntoEavDataStructure(Context).GetEavEntity(entity.EntityID) : null;
            var newValuesTyped = DictionaryToValuesViewModel(newValues);
            foreach (var newValue in newValuesTyped)
            {
                var attribute = attributes.FirstOrDefault(a => a.StaticName == newValue.Key);
                if(attribute != null)
                    Context.Values.UpdateValue(entity, attribute, masterRecord, currentValues, entityModel, newValue.Value, dimensionIds);
            }

            #region if Dimensions are specified, purge/remove specified dimensions for Values that are not in newValues
            if (dimensionIds.Count > 0)
            {
                var attributeMetadataSource = DataSource.GetMetaDataSource(Context.ZoneId, Context.AppId);

                var keys = newValuesTyped.Keys.ToArray();
                // Get all Values that are not present in newValues
                var valuesToPurge = entity.Values.Where(v => !v.ChangeLogIDDeleted.HasValue && !keys.Contains(v.Attribute.StaticName) && v.ValuesDimensions.Any(d => dimensionIds.Contains(d.DimensionID)));
                foreach (var valueToPurge in valuesToPurge)
                {
                    // Don't touch Value if attribute is not visible in UI
                    var attributeMetadata = attributeMetadataSource.GetAssignedEntities(Constants.AssignmentObjectTypeIdFieldProperties, valueToPurge.AttributeID, "@All").FirstOrDefault();
                    if (attributeMetadata != null)
                    {
                        var visibleInEditUi = ((Attribute<bool?>)attributeMetadata["VisibleInEditUI"]).TypedContents;
                        if (visibleInEditUi == false)
                            continue;
                    }

                    // Check if the Value is only used in this supplied dimension (carefull, dont' know what to do if we have multiple dimensions!, must define later)
                    // if yes, delete/invalidate the value
                    if (valueToPurge.ValuesDimensions.Count == 1)
                        valueToPurge.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
                    // if now, remove dimension from Value
                    else
                    {
                        foreach (var valueDimension in valueToPurge.ValuesDimensions.Where(d => dimensionIds.Contains(d.DimensionID)).ToList())
                            valueToPurge.ValuesDimensions.Remove(valueDimension);
                    }
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Convert IOrderedDictionary to <see cref="Dictionary{String, ValueViewModel}" /> (for backward capability)
        /// </summary>
        private Dictionary<string, ValueToImport> DictionaryToValuesViewModel(IDictionary newValues)
        {
            if (newValues is Dictionary<string, ValueToImport>)
                return (Dictionary<string, ValueToImport>)newValues;

            return newValues.Keys.Cast<object>().ToDictionary(key => key.ToString(), key => new ValueToImport { ReadOnly = false, Value = newValues[key] });
        }

        #region Delete Commands

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(int repositoryId)
        {
            return DeleteEntity(GetEntity(repositoryId));
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        public bool DeleteEntity(Guid entityGuid)
        {
            return DeleteEntity(GetEntity(entityGuid));
        }

        /// <summary>
        /// Delete an Entity
        /// </summary>
        internal bool DeleteEntity(Entity entity, bool autoSave = true)
        {
            if (entity == null)
                return false;

            #region Delete Related Records (Values, Value-Dimensions, Relationships)
            // Delete all Value-Dimensions
            var valueDimensions = entity.Values.SelectMany(v => v.ValuesDimensions).ToList();
            valueDimensions.ForEach(Context.SqlDb.DeleteObject);
            // Delete all Values
            entity.Values.ToList().ForEach(Context.SqlDb.DeleteObject);
            // Delete all Parent-Relationships
            entity.EntityParentRelationships.ToList().ForEach(Context.SqlDb.DeleteObject);
            #endregion

            // If entity was Published, set Deleted-Flag
            if (entity.IsPublished)
            {
                entity.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
                // Also delete the Draft (if any)
                var draftEntityId = Context.Publishing.GetDraftEntityId(entity.EntityID);
                if (draftEntityId.HasValue)
                    DeleteEntity(draftEntityId.Value);
            }
            // If entity was a Draft, really delete that Entity
            else
            {
                // Delete all Child-Relationships
                entity.EntityChildRelationships.ToList().ForEach(Context.SqlDb.DeleteObject);
                Context.SqlDb.DeleteObject(entity);
            }

            if (autoSave)
                Context.SqlDb.SaveChanges();

            return true;
        }


        public Tuple<bool, string> CanDeleteEntity(int entityId)
        {
            var messages = new List<string>();
            var entityModel = new DbLoadIntoEavDataStructure(Context).GetEavEntity(entityId);

            if (!entityModel.IsPublished && entityModel.GetPublished() == null)	// allow Deleting Draft-Only Entity always
                return new Tuple<bool, string>(true, null);

            var entityChild = Context.SqlDb.EntityRelationships.Where(r => r.ChildEntityID == entityId).Select(r => r.ParentEntityID).ToList();
            if (entityChild.Any())
                messages.Add(string.Format("found {0} child relationships: {1}.", entityChild.Count, string.Join(", ", entityChild)));

            var assignedEntitiesFieldProperties = GetEntitiesInternal(Constants.AssignmentObjectTypeIdFieldProperties, entityId).Select(e => e.EntityID).ToList();
            if (assignedEntitiesFieldProperties.Any())
                messages.Add(string.Format("found {0} assigned field property entities: {1}.", assignedEntitiesFieldProperties.Count, string.Join(", ", assignedEntitiesFieldProperties)));

            var assignedEntitiesDataPipeline = GetEntitiesInternal(Constants.AssignmentObjectTypeEntity, entityId).Select(e => e.EntityID).ToList();
            if (assignedEntitiesDataPipeline.Any())
                messages.Add(string.Format("found {0} assigned data-pipeline entities: {1}.", assignedEntitiesDataPipeline.Count, string.Join(", ", assignedEntitiesDataPipeline)));

            return Tuple.Create(!messages.Any(), string.Join(" ", messages));
        }

        #endregion

    }
}
