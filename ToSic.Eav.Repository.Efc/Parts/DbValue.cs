using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbValue : BllCommandBase
    {
        public DbValue(DbDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
        /// </summary>
        internal void CloneEntityValues(ToSicEavEntities source, ToSicEavEntities target)
        {
            // Clear values on target (including Dimensions). Must be done in separate steps, would cause unallowed null-Foreign-Keys
            if (target.ToSicEavValues.Any())
            {
                foreach (var eavValue in target.ToSicEavValues)
                    eavValue.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            }

            // Add all Values with Dimensions
            foreach (var eavValue in source.ToSicEavValues.ToList())
            {
                var value = eavValue;// 2017-04-19 todo validate // DbContext.DbS.CopyEfEntity(eavValue);
                // copy Dimensions
                foreach (var valuesDimension in eavValue.ToSicEavValuesDimensions)
                    value.ToSicEavValuesDimensions.Add(new ToSicEavValuesDimensions
                    {
                        DimensionId = valuesDimension.DimensionId,
                        ReadOnly = valuesDimension.ReadOnly
                    });

                target.ToSicEavValues.Add(value);
            }

            #region copy relationships
            // note the related Entities are managed in the EntityParentRelationships. not sure why though
            target.RelationshipsWithThisAsParent/*EntityParentRelationships*/.Clear();

            // Add all Related Entities
            foreach (var entityParentRelationship in source.RelationshipsWithThisAsParent/*.EntityParentRelationships*/)
                target.RelationshipsWithThisAsParent/*.EntityParentRelationships*/.Add(new ToSicEavEntityRelationships
                {
                    AttributeId = entityParentRelationship.AttributeId,
                    ChildEntityId = entityParentRelationship.ChildEntityId,
                    SortOrder = entityParentRelationship.SortOrder
                });
            #endregion
        }

        /// <summary>
        /// Add a new Value
        /// </summary>
        internal ToSicEavValues AddValue(ToSicEavEntities entity, int attributeId, string value, bool autoSave = true)
        {
            var changeId = DbContext.Versioning.GetChangeLogId();

            var newValue = new ToSicEavValues
            {
                AttributeId = attributeId,
                Entity = entity,
                Value = value,
                ChangeLogCreated = changeId
            };

            DbContext.SqlDb.Add(newValue);
            if (autoSave)
                DbContext.SqlDb.SaveChanges();
            return newValue;
        }


        /// <summary>
        /// Update a Value
        /// </summary>
        internal void UpdateValue(ToSicEavValues currentValue, string value, int changeId, bool autoSave = true)
        {
            // only if value has changed
            if (currentValue.Value.Equals(value))
                return;

            currentValue.Value = value;
            currentValue.ChangeLogModified = changeId;
            currentValue.ChangeLogDeleted = null;

            if (autoSave)
                DbContext.SqlDb.SaveChanges();
        }

        #region Update Values
        /// <summary>
        /// Update a Value when using IValueImportModel. Returns the Updated Value (for simple Values) or null (for Entity-Values)
        /// </summary>
        internal object UpdateValueByImport(ToSicEavEntities entityInDb, ToSicEavAttributes attribute, List<ToSicEavValues> currentValues, IImpValue newImpValue)
        {
            switch (attribute.Type)
            {
                // Handle Entity Relationships - they're stored in own tables
                case "Entity":
                    if (newImpValue is ImpValue<List<Guid>> || newImpValue is ImpValue<List<Guid?>>)
                    {
                        // often the list is not nullable, but sometimes it is - the further processing always expects nullable Guids
                        var guidList = (newImpValue as ImpValue<List<Guid>>)?.Value.Select(p => (Guid?) p) 
                            ?? ((ImpValue<List<Guid?>>)newImpValue).Value.Select(p => p);
                        DbContext.Relationships.AddToQueue(attribute.AttributeId, guidList.ToList(), null, entityInDb.EntityId);
                    }
                    // old version with less clear code
                    //if (newImpValue is ImpValue<List<Guid?>>)
                    //    DbContext.Relationships.UpdateEntityRelationships(attribute.AttributeId, ((ImpValue<List<Guid?>>)newImpValue).Value.Select(p => (Guid?)p).ToList(), null, entityInDb.EntityId);
                    else
                        throw new NotSupportedException("UpdateValue() for Attribute " + attribute.StaticName + " with newValue of type" + newImpValue.GetType() + " not supported. Expected List<Guid>");

                    return null;
                // Handle simple values in Values-Table
                default:
                    // masterRecord can be true or false, it's not used when valueDimensions is specified
                    return UpdateSimpleValue(attribute, entityInDb, null, true, GetTypedValue(newImpValue, attribute.Type, attribute.StaticName), null, false, currentValues, null, newImpValue.ValueDimensions);
            }
        }

        /// <summary>
        /// Get typed value from ValueImportModel
        /// </summary>
        /// <param name="impValue">Value to convert</param>
        /// <param name="attributeType">Attribute Type</param>
        /// <param name="attributeStaticName">Attribute StaticName</param>
        /// <param name="multiValuesSeparator">Indicates whehter returned value should be convertable to a human readable string - currently only used for GetEntityVersionValues()</param>
        internal object GetTypedValue(IImpValue impValue, string attributeType = null, string attributeStaticName = null, string multiValuesSeparator = null)
        {
            object typedValue;

            // make sure we have the right type...
            var type = AttributeTypeEnum.Undefined;
            if(attributeType != null && Enum.IsDefined(typeof(AttributeTypeEnum), attributeType))
                type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);

            if ((type == AttributeTypeEnum.Boolean || type == AttributeTypeEnum.Undefined) && impValue is ImpValue<bool?>) 
                typedValue = ((ImpValue<bool?>)impValue).Value;
            else if ((type == AttributeTypeEnum.DateTime || type == AttributeTypeEnum.Undefined) && impValue is ImpValue<DateTime?>)
                typedValue = ((ImpValue<DateTime?>)impValue).Value;
            else if ((type == AttributeTypeEnum.Number || type == AttributeTypeEnum.Undefined) && impValue is ImpValue<decimal?>)
                typedValue = ((ImpValue<decimal?>)impValue).Value;
            else if ((type == AttributeTypeEnum.String
                || type == AttributeTypeEnum.Hyperlink
                || type == AttributeTypeEnum.Custom
                || type == AttributeTypeEnum.Undefined) && impValue is ImpValue<string>) 
                typedValue = ((ImpValue<string>)impValue).Value;
            else if (impValue is ImpValue<List<Guid>> && multiValuesSeparator != null)
            {
                var entityGuids = ((ImpValue<List<Guid>>)impValue).Value;
                typedValue = EntityGuidsToString(entityGuids, multiValuesSeparator);
            }
            else
                throw new NotSupportedException($"GetTypedValue() for Attribute {attributeStaticName} (Type: {attributeType}) with newValue of type {impValue.GetType()} not supported.");
            return typedValue;
        }

        private string EntityGuidsToString(IEnumerable<Guid> entityGuids, string separator = ", ", string format = "{0} (EntityId: {1})")
        {
            var guidIds = entityGuids.ToDictionary(k => k, v => (int?)null);
            foreach (var entityGuid in guidIds.ToList())
            {
                var firstEntityId = DbContext.Entities.GetEntitiesByGuid(entityGuid.Key).Select(e => (int?)e.EntityId).FirstOrDefault();
                if (firstEntityId != null)
                    guidIds[entityGuid.Key] = firstEntityId;
            }
            return string.Join(separator, guidIds.Select(e => string.Format(format, e.Key, e.Value)));
        }

        /// <summary>
        /// Update a Value 
        /// </summary>
        internal void UpdateValue(ToSicEavEntities currentEntity, ToSicEavAttributes attribute, bool masterRecord, List<ToSicEavValues> currentValues, IEntity entityModel, ImpValueInside newValue, ICollection<int> dimensionIds)
        {
            switch (attribute.Type)
            {
                // Handle Entity Relationships - they're stored in own tables
                case "Entity":
                    var entityIds = newValue.Value is int?[] ? (int?[])newValue.Value : ((int[])newValue.Value).Select(v => (int?)v).ToArray();
                    DbContext.Relationships.UpdateEntityRelationships(attribute.AttributeId, entityIds, currentEntity);
                    break;
                // Handle simple values in Values-Table
                default:
                    UpdateSimpleValue(attribute, currentEntity, dimensionIds, masterRecord, newValue.Value, newValue.ValueId, newValue.ReadOnly, currentValues, entityModel);
                    break;
            }
        }

        /// <summary>
        /// Update a Value in the Values-Table
        /// </summary>
        private ToSicEavValues UpdateSimpleValue(ToSicEavAttributes attribute, ToSicEavEntities entity, ICollection<int> dimensionIds, bool masterRecord, object newValue, int? valueId, bool readOnly, List<ToSicEavValues> currentValues, IEntity entityModel, IEnumerable<ImportExport.Models.ImpDims> valueDimensions = null)
        {
            var newValueSerialized = HelpersToRefactor.SerializeValue(newValue);
            var changeId = DbContext.Versioning.GetChangeLogId();

            // Get Value or create new one
            var value = GetOrCreateValue(attribute, entity, masterRecord, valueId, readOnly, currentValues, entityModel, newValueSerialized, changeId, valueDimensions);

            #region Update DimensionIds on this and other values

            // Update Dimensions as specified by Import
            if (valueDimensions != null && valueDimensions.Any())
            {
                var valueDimensionsToDelete = value.ToSicEavValuesDimensions.ToList();
                // loop all specified Dimensions, add or update it for this value
                foreach (var valueDimension in valueDimensions)
                {
                    // ToDo: 2bg Log Error but continue
                    var dimensionId = DbContext.Dimensions.GetDimensionId(null, valueDimension.DimensionExternalKey);
                    if (dimensionId == 0)
                        throw new Exception("Dimension " + valueDimension.DimensionExternalKey + " not found. EntityId: " + entity.EntityId + " Attribute-StaticName: " + attribute.StaticName);

                    var existingValueDimension = value.ToSicEavValuesDimensions.SingleOrDefault(v => v.DimensionId == dimensionId);
                    if (existingValueDimension == null)
                        value.ToSicEavValuesDimensions.Add(new ToSicEavValuesDimensions { DimensionId = dimensionId, ReadOnly = valueDimension.ReadOnly });
                    else
                    {
                        valueDimensionsToDelete.Remove(valueDimensionsToDelete.Single(vd => vd.DimensionId == dimensionId));
                        existingValueDimension.ReadOnly = valueDimension.ReadOnly;
                    }
                }

                // remove old dimensions
                DbContext.SqlDb.RemoveRange(valueDimensionsToDelete); // 2017-04-19 todo validate // valueDimensionsToDelete.ForEach(DbContext.SqlDb.DeleteObject);
            }
            // Update Dimensions as specified on the whole Entity
            else if (dimensionIds != null)
            {
                #region Ensure specified Dimensions are updated/added (whether Value has changed or not)
                // Update existing ValuesDimensions
                foreach (var valueDimension in value.ToSicEavValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionId)))
                {
                    // ReSharper disable RedundantCheckBeforeAssignment
                    // Check to prevent unneeded DB queries
                    if (valueDimension.ReadOnly != readOnly)
                        // ReSharper restore RedundantCheckBeforeAssignment
                        valueDimension.ReadOnly = readOnly;
                }

                // Add new ValuesDimensions
                foreach (var dimensionId in dimensionIds.Where(i => value.ToSicEavValuesDimensions.All(d => d.DimensionId != i)))
                    value.ToSicEavValuesDimensions.Add(new ToSicEavValuesDimensions { DimensionId = dimensionId, ReadOnly = readOnly });

                #endregion

                // Remove current Dimension(s) from other Values
                if (!masterRecord)
                {
                    // Get other Values for current Attribute having all Current Dimensions assigned
                    var otherValuesWithCurrentDimensions = currentValues.Where(v => v.AttributeId == attribute.AttributeId && v.ValueId != value.ValueId && dimensionIds.All(d => v.ToSicEavValuesDimensions.Select(vd => vd.DimensionId).Contains(d)));
                    foreach (var otherValue in otherValuesWithCurrentDimensions)
                    {
                        foreach (var valueDimension in otherValue.ToSicEavValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionId)).ToList())
                        {
                            // if only one Dimension assigned, mark this value as deleted
                            if (otherValue.ToSicEavValuesDimensions.Count == 1)
                                otherValue.ChangeLogDeleted = changeId;

                            otherValue.ToSicEavValuesDimensions.Remove(valueDimension);
                        }
                    }
                }
            }
            #endregion

            return value;
        }

        /// <summary>
        /// Get an EavValue for specified EntityId etc. or create a new one. Uses different mechanism when running an Import or ValueId is specified.
        /// </summary>
        private ToSicEavValues GetOrCreateValue(ToSicEavAttributes attribute, ToSicEavEntities entity, bool masterRecord, int? valueId, bool readOnly, List<ToSicEavValues> currentValues, IEntity entityModel, string newValueSerialized, int changeId, IEnumerable<ImportExport.Models.ImpDims> valueDimensions)
        {
            ToSicEavValues value = null;
            // if Import-Dimension(s) are Specified
            if (valueDimensions != null && valueDimensions.Any())
            {
                // Get first value having first Dimension or add new value
                value = currentValues.FirstOrDefault(v => v.ChangeLogDeleted == null && v.Attribute.StaticName == attribute.StaticName && v.ToSicEavValuesDimensions.Any(d => d.Dimension.ExternalKey.Equals(valueDimensions.First().DimensionExternalKey, StringComparison.InvariantCultureIgnoreCase))) ??
                        AddValue(entity, attribute.AttributeId, newValueSerialized, autoSave: false);
            }
            // if ValueId & EntityId is specified, use this Value
            else if (valueId.HasValue && entity.EntityId != 0)
            {
                value = currentValues.Single(v => v.ValueId == valueId.Value && v.Attribute.StaticName == attribute.StaticName);
                // If Master, ensure ValueId is from Master!
                var attributeModel = (IAttributeManagement)entityModel.Attributes.SingleOrDefault(a => a.Key == attribute.StaticName).Value;
                if (masterRecord && value.ValueId != attributeModel.DefaultValue.ValueId)
                    throw new Exception("Master Record cannot use a ValueId rather ValueId from Master. Attribute-StaticName: " + attribute.StaticName);
            }
            // Find Value (if not specified) or create new one
            else
            {
                if (masterRecord) // if true, don't create new Value (except no one exists)
                    value = currentValues.Where(v => v.AttributeId == attribute.AttributeId).OrderBy(a => a.ChangeLogCreated).FirstOrDefault();

                // if no Value found, create new one
                if (value == null)
                {
                    if (!masterRecord && currentValues.All(v => v.AttributeId != attribute.AttributeId))
                        // if updating Additional-Entity but Default-Entity doesn't have any atom
                        throw new Exception("Update of a \"" + attribute.StaticName +
                                            "\" is not allowed. You must first updated this Value for the Default-Entity.");

                    value = AddValue(entity, attribute.AttributeId, newValueSerialized, autoSave: false);
                }
            }

            // Update old/existing Value
            if (value.ValueId != 0 || entity.EntityId == 0)
            {
                if (!readOnly)
                    UpdateValue(value, newValueSerialized, changeId, false);
            }
            return value;
        }






        #endregion


    }
}
