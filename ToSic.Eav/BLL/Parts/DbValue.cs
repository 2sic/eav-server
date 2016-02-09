using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.BLL.Parts
{
    public class DbValue : BllCommandBase
    {
        public DbValue(EavDataController cntx) : base(cntx)
        {
        }

        /// <summary>
        /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
        /// </summary>
        internal void CloneEntityValues(Entity source, Entity target)
        {
            // Clear values on target (including Dimensions). Must be done in separate steps, would cause unallowed null-Foreign-Keys
            if (target.Values.Any())
            {
                foreach (var eavValue in target.Values)
                    eavValue.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
            }

            // Add all Values with Dimensions
            foreach (var eavValue in source.Values.ToList())
            {
                var value = Context.DbS.CopyEfEntity(eavValue);
                // copy Dimensions
                foreach (var valuesDimension in eavValue.ValuesDimensions)
                    value.ValuesDimensions.Add(new ValueDimension
                    {
                        DimensionID = valuesDimension.DimensionID,
                        ReadOnly = valuesDimension.ReadOnly
                    });

                target.Values.Add(value);
            }

            target.EntityParentRelationships.Clear();
            // Add all Related Entities
            foreach (var entityParentRelationship in source.EntityParentRelationships)
                target.EntityParentRelationships.Add(new EntityRelationship
                {
                    AttributeID = entityParentRelationship.AttributeID,
                    ChildEntityID = entityParentRelationship.ChildEntityID
                });
        }

        /// <summary>
        /// Add a new Value
        /// </summary>
        internal EavValue AddValue(Entity entity, int attributeId, string value, bool autoSave = true)
        {
            var changeId = Context.Versioning.GetChangeLogId();

            var newValue = new EavValue
            {
                AttributeID = attributeId,
                Entity = entity,
                Value = value,
                ChangeLogIDCreated = changeId
            };

            Context.SqlDb.AddToValues(newValue);
            if (autoSave)
                Context.SqlDb.SaveChanges();
            return newValue;
        }


        /// <summary>
        /// Update a Value
        /// </summary>
        internal void UpdateValue(EavValue currentValue, string value, int changeId, bool autoSave = true)
        {
            // only if value has changed
            if (currentValue.Value.Equals(value))
                return;

            currentValue.Value = value;
            currentValue.ChangeLogIDModified = changeId;
            currentValue.ChangeLogIDDeleted = null;

            if (autoSave)
                Context.SqlDb.SaveChanges();
        }

        #region Update Values
        /// <summary>
        /// Update a Value when using IValueImportModel. Returns the Updated Value (for simple Values) or null (for Entity-Values)
        /// </summary>
        internal object UpdateValueByImport(Entity entityInDb, Attribute attribute, List<EavValue> currentValues, IValueImportModel newValue)
        {
            switch (attribute.Type)
            {
                // Handle Entity Relationships - they're stored in own tables
                case "Entity":
                    if (newValue is ValueImportModel<List<Guid>>)
                        Context.Relationships.UpdateEntityRelationships(attribute.AttributeID, ((ValueImportModel<List<Guid>>)newValue).Value.Select(p => (Guid?)p).ToList(), null /* entityInDb.EntityGUID, null */, entityInDb.EntityID);
                    if (newValue is ValueImportModel<List<Guid?>>)
                        Context.Relationships.UpdateEntityRelationships(attribute.AttributeID, ((ValueImportModel<List<Guid?>>)newValue).Value.Select(p => p).ToList(), null, entityInDb.EntityID);
                    else
                        throw new NotSupportedException("UpdateValue() for Attribute " + attribute.StaticName + " with newValue of type" + newValue.GetType() + " not supported. Expected List<Guid>");

                    return null;
                // Handle simple values in Values-Table
                default:
                    // masterRecord can be true or false, it's not used when valueDimensions is specified
                    return UpdateSimpleValue(attribute, entityInDb, null, true, GetTypedValue(newValue, attribute.Type, attribute.StaticName), null, false, currentValues, null, newValue.ValueDimensions);
            }
        }

        /// <summary>
        /// Get typed value from ValueImportModel
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="attributeType">Attribute Type</param>
        /// <param name="attributeStaticName">Attribute StaticName</param>
        /// <param name="multiValuesSeparator">Indicates whehter returned value should be convertable to a human readable string - currently only used for GetEntityVersionValues()</param>
        internal object GetTypedValue(IValueImportModel value, string attributeType = null, string attributeStaticName = null, string multiValuesSeparator = null)
        {
            object typedValue;

            // make sure we have the right type...
            var type = AttributeTypeEnum.Undefined;
            if(attributeType != null && Enum.IsDefined(typeof(AttributeTypeEnum), attributeType))
                type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);

            if ((type == AttributeTypeEnum.Boolean || type == AttributeTypeEnum.Undefined) && value is ValueImportModel<bool?>) 
                typedValue = ((ValueImportModel<bool?>)value).Value;
            else if ((type == AttributeTypeEnum.DateTime || type == AttributeTypeEnum.Undefined) && value is ValueImportModel<DateTime?>)
                typedValue = ((ValueImportModel<DateTime?>)value).Value;
            else if ((type == AttributeTypeEnum.Number || type == AttributeTypeEnum.Undefined) && value is ValueImportModel<decimal?>)
                typedValue = ((ValueImportModel<decimal?>)value).Value;
            else if ((type == AttributeTypeEnum.String
                || type == AttributeTypeEnum.Hyperlink
                || type == AttributeTypeEnum.Custom
                || type == AttributeTypeEnum.Undefined) && value is ValueImportModel<string>) 
                typedValue = ((ValueImportModel<string>)value).Value;
            else if (value is ValueImportModel<List<Guid>> && multiValuesSeparator != null)
            {
                var entityGuids = ((ValueImportModel<List<Guid>>)value).Value;
                typedValue = EntityGuidsToString(entityGuids, multiValuesSeparator);
            }
            else
                throw new NotSupportedException(string.Format("GetTypedValue() for Attribute {0} (Type: {1}) with newValue of type {2} not supported.", attributeStaticName, attributeType, value.GetType()));
            return typedValue;
        }

        private string EntityGuidsToString(IEnumerable<Guid> entityGuids, string separator = ", ", string format = "{0} (EntityId: {1})")
        {
            var guidIds = entityGuids.ToDictionary(k => k, v => (int?)null);
            foreach (var entityGuid in guidIds.ToList())
            {
                var firstEntityId = Context.Entities.GetEntitiesByGuid(entityGuid.Key).Select(e => (int?)e.EntityID).FirstOrDefault();
                if (firstEntityId != null)
                    guidIds[entityGuid.Key] = firstEntityId;
            }
            return string.Join(separator, guidIds.Select(e => string.Format(format, e.Key, e.Value)));
        }

        /// <summary>
        /// Update a Value when using ValueViewModel
        /// </summary>
        internal void UpdateValue(Entity currentEntity, Attribute attribute, bool masterRecord, List<EavValue> currentValues, IEntity entityModel, ValueToImport newValue, ICollection<int> dimensionIds)
        {
            switch (attribute.Type)
            {
                // Handle Entity Relationships - they're stored in own tables
                case "Entity":
                    var entityIds = newValue.Value is int?[] ? (int?[])newValue.Value : ((int[])newValue.Value).Select(v => (int?)v).ToArray();
                    Context.Relationships.UpdateEntityRelationships(attribute.AttributeID, entityIds, currentEntity);
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
        private EavValue UpdateSimpleValue(Attribute attribute, Entity entity, ICollection<int> dimensionIds, bool masterRecord, object newValue, int? valueId, bool readOnly, List<EavValue> currentValues, IEntity entityModel, IEnumerable<Import.ValueDimension> valueDimensions = null)
        {
            var newValueSerialized = Eav.HelpersToRefactor.SerializeValue(newValue);
            var changeId = Context.Versioning.GetChangeLogId();

            // Get Value or create new one
            var value = GetOrCreateValue(attribute, entity, masterRecord, valueId, readOnly, currentValues, entityModel, newValueSerialized, changeId, valueDimensions);

            #region Update DimensionIds on this and other values

            // Update Dimensions as specified by Import
            if (valueDimensions != null && valueDimensions.Any())
            {
                var valueDimensionsToDelete = value.ValuesDimensions.ToList();
                // loop all specified Dimensions, add or update it for this value
                foreach (var valueDimension in valueDimensions)
                {
                    // ToDo: 2bg Log Error but continue
                    var dimensionId = Context.Dimensions.GetDimensionId(null, valueDimension.DimensionExternalKey);
                    if (dimensionId == 0)
                        throw new Exception("Dimension " + valueDimension.DimensionExternalKey + " not found. EntityId: " + entity.EntityID + " Attribute-StaticName: " + attribute.StaticName);

                    var existingValueDimension = value.ValuesDimensions.SingleOrDefault(v => v.DimensionID == dimensionId);
                    if (existingValueDimension == null)
                        value.ValuesDimensions.Add(new ValueDimension { DimensionID = dimensionId, ReadOnly = valueDimension.ReadOnly });
                    else
                    {
                        valueDimensionsToDelete.Remove(valueDimensionsToDelete.Single(vd => vd.DimensionID == dimensionId));
                        existingValueDimension.ReadOnly = valueDimension.ReadOnly;
                    }
                }

                // remove old dimensions
                valueDimensionsToDelete.ForEach(Context.SqlDb.DeleteObject);
            }
            // Update Dimensions as specified on the whole Entity
            else if (dimensionIds != null)
            {
                #region Ensure specified Dimensions are updated/added (whether Value has changed or not)
                // Update existing ValuesDimensions
                foreach (var valueDimension in value.ValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionID)))
                {
                    // ReSharper disable RedundantCheckBeforeAssignment
                    // Check to prevent unneeded DB queries
                    if (valueDimension.ReadOnly != readOnly)
                        // ReSharper restore RedundantCheckBeforeAssignment
                        valueDimension.ReadOnly = readOnly;
                }

                // Add new ValuesDimensions
                foreach (var dimensionId in dimensionIds.Where(i => value.ValuesDimensions.All(d => d.DimensionID != i)))
                    value.ValuesDimensions.Add(new ValueDimension { DimensionID = dimensionId, ReadOnly = readOnly });

                #endregion

                // Remove current Dimension(s) from other Values
                if (!masterRecord)
                {
                    // Get other Values for current Attribute having all Current Dimensions assigned
                    var otherValuesWithCurrentDimensions = currentValues.Where(v => v.AttributeID == attribute.AttributeID && v.ValueID != value.ValueID && dimensionIds.All(d => v.ValuesDimensions.Select(vd => vd.DimensionID).Contains(d)));
                    foreach (var otherValue in otherValuesWithCurrentDimensions)
                    {
                        foreach (var valueDimension in otherValue.ValuesDimensions.Where(vd => dimensionIds.Contains(vd.DimensionID)).ToList())
                        {
                            // if only one Dimension assigned, mark this value as deleted
                            if (otherValue.ValuesDimensions.Count == 1)
                                otherValue.ChangeLogIDDeleted = changeId;

                            otherValue.ValuesDimensions.Remove(valueDimension);
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
        private EavValue GetOrCreateValue(Attribute attribute, Entity entity, bool masterRecord, int? valueId, bool readOnly, List<EavValue> currentValues, IEntity entityModel, string newValueSerialized, int changeId, IEnumerable<Import.ValueDimension> valueDimensions)
        {
            EavValue value = null;
            // if Import-Dimension(s) are Specified
            if (valueDimensions != null && valueDimensions.Any())
            {
                // Get first value having first Dimension or add new value
                value = currentValues.FirstOrDefault(v => v.ChangeLogIDDeleted == null && v.Attribute.StaticName == attribute.StaticName && v.ValuesDimensions.Any(d => d.Dimension.ExternalKey.Equals(valueDimensions.First().DimensionExternalKey, StringComparison.InvariantCultureIgnoreCase))) ??
                        AddValue(entity, attribute.AttributeID, newValueSerialized, autoSave: false);
            }
            // if ValueID & EntityId is specified, use this Value
            else if (valueId.HasValue && entity.EntityID != 0)
            {
                value = currentValues.Single(v => v.ValueID == valueId.Value && v.Attribute.StaticName == attribute.StaticName);
                // If Master, ensure ValueID is from Master!
                var attributeModel = (IAttributeManagement)entityModel.Attributes.SingleOrDefault(a => a.Key == attribute.StaticName).Value;
                if (masterRecord && value.ValueID != attributeModel.DefaultValue.ValueId)
                    throw new Exception("Master Record cannot use a ValueID rather ValueID from Master. Attribute-StaticName: " + attribute.StaticName);
            }
            // Find Value (if not specified) or create new one
            else
            {
                if (masterRecord) // if true, don't create new Value (except no one exists)
                    value = currentValues.Where(v => v.AttributeID == attribute.AttributeID).OrderBy(a => a.ChangeLogIDCreated).FirstOrDefault();

                // if no Value found, create new one
                if (value == null)
                {
                    if (!masterRecord && currentValues.All(v => v.AttributeID != attribute.AttributeID))
                        // if updating Additional-Entity but Default-Entity doesn't have any atom
                        throw new Exception("Update of a \"" + attribute.StaticName +
                                            "\" is not allowed. You must first updated this Value for the Default-Entity.");

                    value = AddValue(entity, attribute.AttributeID, newValueSerialized, autoSave: false);
                }
            }

            // Update old/existing Value
            if (value.ValueID != 0 || entity.EntityID == 0)
            {
                if (!readOnly)
                    UpdateValue(value, newValueSerialized, changeId, false);
            }
            return value;
        }






        #endregion


    }
}
