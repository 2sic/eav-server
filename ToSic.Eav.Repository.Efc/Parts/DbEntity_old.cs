using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Logging;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity: BllCommandBase
    {

        #region Get Commands

        #endregion

        #region Add Commands
        ///// <summary>
        ///// Import a new Entity
        ///// </summary>
        //internal ToSicEavEntities AddImportEntity(int attributeSetId, Entity entity, bool isPublished, int? publishedTarget)
        //{
        //    return AddEntity(attributeSetId, entity.Attributes, entity.Metadata,
        //        /*0,*/ entity.EntityGuid, null, isPublished, publishedTarget);
        //}
        
        
        /// <summary>
        /// Add a new Entity
        /// </summary>
        public ToSicEavEntities AddEntity(int attributeSetId, IDictionary values, 
            IIsMetadata isMetadata = null, Guid? entityGuid = null, ICollection<int> dimensionIds = null, bool isPublished = true, int? publishedEntityId = null)
        {
            // note: values is either a dictionary <string, object> or <string, IList<IValue>>

            int? keyNumber = isMetadata?.KeyNumber;
            int keyTypeId = isMetadata?.TargetType ?? Constants.NotMetadata;

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
                    //ConfigurationSet = null, // configurationSet,
                    AssignmentObjectTypeId = keyTypeId,
                    KeyNumber = keyNumber,
                    KeyGuid = isMetadata?.KeyGuid, //keyGuid,
                    KeyString = isMetadata?.KeyString, //keyString,
                    //SortOrder = sortOrder,
                    ChangeLogCreated = changeId,
                    ChangeLogModified = changeId,
                    EntityGuid = entityGuid.HasValue && entityGuid.Value != Guid.Empty ? entityGuid.Value : Guid.NewGuid(),
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

            return UpdateAttributesAndPublishing(existingEntityId, values, dimensionIds: dimensionIds, autoSave: false, /*updateLog: updateLog,*/ isPublished: isPublished);

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
        /// <param name="preserveUndefinedValues">Preserve Values if Attribute is not specifeied in NewValues</param>
        /// <param name="isPublished">Is this Entity Published or a draft</param>
        /// <param name="forceNoBranch">this forces the published-state to be applied to the original, without creating a draft-branhc</param>
        /// <returns>the updated Entity</returns>
        public ToSicEavEntities UpdateAttributesAndPublishing(int repositoryId, IDictionary newValues, bool autoSave = true, ICollection<int> dimensionIds = null, /*List<ImportLogItem> updateLog = null,*/ bool preserveUndefinedValues = true, bool isPublished = true, bool forceNoBranch = false)
        {
            var entity = DbContext.Entities.GetDbEntity(repositoryId);
            var draftEntityId = DbContext.Publishing.GetDraftBranchEntityId(repositoryId);

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
                entity = DbContext.Publishing.OLDClearDraftBranchAndSetPublishedState(repositoryId, isPublished); // must save intermediate because otherwise we get duplicate IDs
            }
            #endregion


            if (dimensionIds == null)
                dimensionIds = new List<int>(0);

            // Load all Attributes and current Values - .ToList() to prevent (slow) lazy loading
            var dbValues = entity.EntityId != 0
                ? DbContext.SqlDb.ToSicEavValues
                    .Include(x => x.Attribute)
                    .Include(x => x.ToSicEavValuesDimensions)
                        .ThenInclude(d => d.Dimension)
                    .Where(v => v.EntityId == entity.EntityId && v.ChangeLogDeleted == null)
                    .ToList()
                : entity.ToSicEavValues.ToList();

            // Update Values from Import Model
            var newValuesImport = newValues as Dictionary<string, IAttribute>;
            if (newValuesImport == null) // convert to a Dictionary<string, IAttribute>;
            {
                var dicStrObj = (Dictionary<string, object>) newValues;
                newValuesImport = dicStrObj.ConvertToAttributes();
            }

            if (newValuesImport != null)
                UpdateEntityWithIAttributes(entity, newValuesImport, /*updateLog,*//* attributes,*/ dbValues, preserveUndefinedValues);
            // Update Values from ValueViewModel
            //else
            //    UpdateEntityDefault(entity, (Dictionary<string, object>)newValues, dimensionIds/*, masterRecord*/, attributes, dbValues);


            entity.ChangeLogModified = DbContext.Versioning.GetChangeLogId();

            DbContext.SqlDb.SaveChanges();	// must save now to generate EntityModel afterward for DataTimeline

            DbContext.Versioning.SaveEntity(entity.EntityId, entity.EntityGuid, useDelayedSerialize: true);

            return entity;
        }

        /// <summary>
        /// Update an Entity when using the Import
        /// </summary>
        private void UpdateEntityWithIAttributes(ToSicEavEntities dbEntity, Dictionary<string, IAttribute> newAttribs, /*List<ToSicEavAttributes> attributeList,*/ List<ToSicEavValues> currentValues, bool keepAttributesMissingInImport)
        {
            var attributeList = DbContext.AttributesDefinition.GetAttributeDefinitions(dbEntity.AttributeSetId).ToList();

            if (ImportLog == null)
                throw new ArgumentNullException(nameof(ImportLog), "When Calling UpdateEntity() with newValues of Type IValueImportModel updateLog must be set.");

            // track updated values to remove values that were not updated automatically
            var updatedValueIds = new List<int>();
            var updatedAttributeIds = new List<int>();
            foreach (var newAttrib in newAttribs)
            {
                #region Check if this field definition exists, else log/skip this field if not found
                var attribute = attributeList.SingleOrDefault(a => string.Equals(a.StaticName, newAttrib.Key, StringComparison.InvariantCultureIgnoreCase));
                if (attribute == null) // Attribute not found
                {
                    // Log Warning for all Values
                    ImportLog.AddRange(newAttrib.Value.Values.Select(v => new ImportLogItem(EventLogEntryType.Warning, "Attribute not found for Value") { ImpAttribute =newAttrib.Key, ImpValue = v, }));
                    continue;
                }
                #endregion

                updatedAttributeIds.Add(attribute.AttributeId);

                // Go through each value / dimensions combination
                foreach (var newSingleValue in newAttrib.Value.Values)
                {
                    try
                    {
                        var updatedValue = DbContext.Values.UpdateAttributeValues(dbEntity, attribute, currentValues, newSingleValue);
                        var updatedEavValue = updatedValue as ToSicEavValues;
                        if (updatedEavValue != null)
                            updatedValueIds.Add(updatedEavValue.ValueId);
                    }
                    catch (Exception ex)
                    {
                        ImportLog.Add(new ImportLogItem(EventLogEntryType.Error, "Update Entity-Value failed") { ImpAttribute = newAttrib.Key, ImpValue = newSingleValue, Exception = ex});
                    }
                }
            }

            // remove all existing values that were not updated
            // Logic should be:
            // Of all values - skip the ones we just modified and those which are deleted
            var untouchedValues = dbEntity.ToSicEavValues.Where(
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


        //continue here - trying to make updateentitydefault unnecessary - but must be sure that it doesn't have important logic which is missing on the other site

        // 2017-06 simplifysave 2dm
        /// <summary>
        /// Update an Entity when not using the Import
        /// </summary>
        private void UpdateEntityDefault(ToSicEavEntities entity, IDictionary<string, object> newValues, ICollection<int> dimensionIds, List<ToSicEavAttributes> attributes, List<ToSicEavValues> dbValues)
        {
            var newValuesTyped = newValues;// DictionaryToValuesViewModel(newValues);
            foreach (var newValue in newValuesTyped)
            {
                var attribute = attributes.FirstOrDefault(a => a.StaticName == newValue.Key);
                if (attribute != null)
                    DbContext.Values.UpdateAttributeValues(entity, attribute, dbValues, newValue.Value, dimensionIds);
            }

            #region if Dimensions are specified, purge/remove specified dimensions for Values that are not in newValues
            if (dimensionIds.Count <= 0) return;

            var attributeMetadataSource = DataSource.GetMetaDataSource(DbContext.ZoneId, DbContext.AppId);

            var keys = newValuesTyped.Keys.ToArray();
            // Get all Values that are not present in newValues
            var valuesToPurge = entity.ToSicEavValues
                .Where(v => !v.ChangeLogDeleted.HasValue
                            && !keys.Contains(v.Attribute.StaticName)
                            && v.ToSicEavValuesDimensions.Any(d => dimensionIds.Contains(d.DimensionId)));
            foreach (var valueToPurge in valuesToPurge)
            {
                // Don't touch Value if attribute is not visible in UI
                var attributeMetadata = attributeMetadataSource.GetAssignedEntities(Constants.MetadataForField, valueToPurge.AttributeId, "@All").FirstOrDefault();
                var visibleInEditUi = ((Attribute<bool?>)attributeMetadata?["VisibleInEditUI"])?.TypedContents;
                if (visibleInEditUi == false)
                    continue;

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

            #endregion
        }
        #endregion
        

    }
}
