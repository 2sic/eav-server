using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
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
            if (target.ToSicEavValues.Any(v => v.ChangeLogDeleted == null))
            {
                foreach (var eavValue in target.ToSicEavValues.Where(v => v.ChangeLogDeleted == null))
                    eavValue.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            }

            // Add all Values with Dimensions
            foreach (var eavValue in source.ToSicEavValues.ToList())
            {
                var value = new ToSicEavValues()
                {
                    AttributeId = eavValue.AttributeId,
                    Value = eavValue.Value,
                    ChangeLogCreated = DbContext.Versioning.GetChangeLogId()
                };// eavValue;// 2017-04-19 todo validate // DbContext.DbS.CopyEfEntity(eavValue);
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
        private ToSicEavValues AddSingleValue(ToSicEavEntities entity, int attributeId, string value)
        {
            var newValue = new ToSicEavValues
            {
                AttributeId = attributeId,
                Entity = entity,
                Value = value,
                //ChangeLogCreated = DbContext.Versioning.GetChangeLogId()
            };

            DbContext.SqlDb.Add(newValue);
            return newValue;
        }


        /// <summary>
        /// Update a Value
        /// </summary>
        internal void UpdateSingleValue(ToSicEavValues currentValue, string value)//, int changeId)
        {
            // only if value has changed
            if (currentValue.Value.Equals(value))
                return;

            currentValue.Value = value;
            //currentValue.ChangeLogModified = changeId;
            currentValue.ChangeLogDeleted = null;
        }

        #region Update Values
        /// <summary>
        /// Update a Value when using IValueImportModel. Returns the Updated Value (for simple Values) or null (for Entity-Values)
        /// </summary>
        internal object UpdateAttributeValues(ToSicEavEntities entityInDb, ToSicEavAttributes attribute, List<ToSicEavValues> currentValues, IValue newImpValue)
        {
            var value = newImpValue.ObjectContents;
            switch (attribute.Type)
            {
                // Handle Entity Relationships - they're stored in own tables
                case "Entity":
                    // 2017-06 simplifysave 2dm
                    // todo: mut check if we could also end up with a List<int?> instead of guid...
                    if (value is List<Guid> || value is List<Guid?>)
                    {
                        var guidList = (value as List<Guid>)?.Select(p => (Guid?) p) ?? ((List<Guid?>)value).Select(p => p);
                        DbContext.Relationships.AddToQueue(attribute.AttributeId, guidList.ToList(), /*null,*/ entityInDb.EntityId);
                    }
                    if (value is List<int> || value is List<int?>)
                    {
                        var entityIds = value as List<int?> ?? ((List<int>) value).Select(v => (int?) v).ToList();
                        DbContext.Relationships.AddToQueue(attribute.AttributeId, entityIds, entityInDb.EntityId);
                    }
                    else
                        throw new NotSupportedException("UpdateValue() for Attribute " + attribute.StaticName +
                                                        " with newValue of type" + newImpValue.GetType() +
                                                        " not supported. Expected List<Guid?> or List<int?>");

                    return null;
                // Handle simple values in Values-Table
                default:
                    // masterRecord can be true or false, it's not used when valueDimensions is specified
                    return UpdateSingleValue(attribute, entityInDb, null, newImpValue.ObjectContents, false, currentValues, newImpValue.Languages);
            }
        }

        // 2017-06 simplifysave 2dm
        /// <summary>
        /// Update a Value 
        /// </summary>
        internal void UpdateAttributeValues(ToSicEavEntities entityInDb, ToSicEavAttributes attribute, List<ToSicEavValues> currentValues, object newValue, ICollection<int> dimensionIds)
        {
            throw new Exception("code shouldn't run any more");
            //switch (attribute.Type)
            //{
            //    // Handle Entity Relationships - they're stored in own tables
            //    case "Entity":
            //        var entityIds = newValue as int?[] ?? ((int[])newValue).Select(v => (int?)v).ToArray();
            //        DbContext.Relationships.UpdateEntityRelationshipsAndSave(attribute.AttributeId, entityIds, entityInDb);
            //        break;
            //    // Handle simple values in Values-Table
            //    default:
            //        UpdateSimpleValue(attribute, entityInDb, dimensionIds, newValue, null, false, currentValues);
            //        break;
            //}
        }

        #region old code, commented out
        ///// <summary>
        ///// Get typed value from ValueImportModel
        ///// </summary>
        ///// <param name="impValue">Value to convert</param>
        ///// <param name="attributeType">Attribute Type</param>
        ///// <param name="attributeStaticName">Attribute StaticName</param>
        ///// <param name="multiValuesSeparator">Indicates whehter returned value should be convertable to a human readable string - currently only used for GetEntityVersionValues()</param>
        //private object GetTypedValue(IValue impValue, string attributeType = null, string attributeStaticName = null)//, string multiValuesSeparator = null)
        //{
        //    object typedValue;

        //    // make sure we have the right type...
        //    var type = AttributeTypeEnum.Undefined;
        //    if(attributeType != null && Enum.IsDefined(typeof(AttributeTypeEnum), attributeType))
        //        type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);

        //    if ((type == AttributeTypeEnum.Boolean || type == AttributeTypeEnum.Undefined) && impValue is Value<bool?>) 
        //        typedValue = ((Value<bool?>)impValue).TypedContents;
        //    else if ((type == AttributeTypeEnum.DateTime || type == AttributeTypeEnum.Undefined) && impValue is Value<DateTime?>)
        //        typedValue = ((Value<DateTime?>)impValue).TypedContents;
        //    else if ((type == AttributeTypeEnum.Number || type == AttributeTypeEnum.Undefined) && impValue is Value<decimal?>)
        //        typedValue = ((Value<decimal?>)impValue).TypedContents;
        //    else if ((type == AttributeTypeEnum.String
        //        || type == AttributeTypeEnum.Hyperlink
        //        || type == AttributeTypeEnum.Custom
        //        || type == AttributeTypeEnum.Undefined) && impValue is Value<string>) 
        //        typedValue = ((Value<string>)impValue).TypedContents;
        //    // 2017-06-14 2dm disabled, as the MultiValueSeparator is always null
        //    //else if (impValue is Value<List<Guid>> && multiValuesSeparator != null)
        //    //{
        //    //    var entityGuids = ((Value<List<Guid>>)impValue).TypedContents;
        //    //    typedValue = EntityGuidsToString(entityGuids, multiValuesSeparator);
        //    //}
        //    else
        //        throw new NotSupportedException($"GetTypedValue() for Attribute {attributeStaticName} (Type: {attributeType}) with newValue of type {impValue.GetType()} not supported.");
        //    return typedValue;
        //}

        //private string EntityGuidsToString(IEnumerable<Guid> entityGuids, string separator = ", ", string format = "{0} (EntityId: {1})")
        //{
        //    var guidIds = entityGuids.ToDictionary(k => k, v => (int?)null);
        //    foreach (var entityGuid in guidIds.ToList())
        //    {
        //        var firstEntityId = DbContext.Entities.GetEntitiesByGuid(entityGuid.Key).Select(e => (int?)e.EntityId).FirstOrDefault();
        //        if (firstEntityId != null)
        //            guidIds[entityGuid.Key] = firstEntityId;
        //    }
        //    return string.Join(separator, guidIds.Select(e => string.Format(format, e.Key, e.Value)));
        //}

        #endregion

        /// <summary>
        /// Update a Value in the Values-Table
        /// </summary>
        private ToSicEavValues UpdateSingleValue(ToSicEavAttributes attribute, ToSicEavEntities entity, ICollection<int> dimensionIds, object newValue, /*int? valueId,*/ bool readOnly, List<ToSicEavValues> dbValues, /*IEntity entityModel,*/ IEnumerable<ILanguage> valueDimensions = null)
        {
            var newValueSerialized = HelpersToRefactor.SerializeValue(newValue);

            // Get Value or create new one
            var value = SaveValue(entity, /*valueId, readOnly,*/ attribute, dbValues, newValueSerialized, valueDimensions);

            #region Update DimensionIds on this and other values

            // Update Dimensions as specified by Import
            if (valueDimensions != null) // 2017-06-13 2dm - must also do if list is empty, to remove old parts... && valueDimensions.Any())
            {
                var valueDimensionsToDelete = value.ToSicEavValuesDimensions.ToList();
                // loop all specified Dimensions, add or update it for this value
                foreach (var valueDimension in valueDimensions)
                {
                    // ToDo: 2bg Log Error but continue
                    var dimensionId = DbContext.Dimensions.GetDimensionId(null, valueDimension.Key);
                    if (dimensionId == 0)
                        throw new Exception("Dimension " + valueDimension.Key + " not found. EntityId: " + entity.EntityId + " Attribute-StaticName: " + attribute.StaticName);

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
                DbContext.SqlDb.RemoveRange(valueDimensionsToDelete);
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

            }
            #endregion

            return value;
        }

        /// <summary>
        /// Get an EavValue for specified EntityId etc. or create a new one. Uses different mechanism when running an Import or ValueId is specified.
        /// </summary>
        private ToSicEavValues SaveValue(ToSicEavEntities entity, ToSicEavAttributes attribute, List<ToSicEavValues> dbValues, string newValueSerialized, IEnumerable<ILanguage> valueDimensions)
        {
            ToSicEavValues value;
            // if Import-Dimension(s) are Specified
            if (valueDimensions != null && valueDimensions.Any())
            {
                // Get first value having first Dimension or add new value
                value = dbValues.FirstOrDefault(v =>
                    v.ChangeLogDeleted == null
                    && v.Attribute.StaticName == attribute.StaticName
                    && v.ToSicEavValuesDimensions.Any(d =>
                        d.Dimension.ExternalKey.Equals(valueDimensions.First().Key,
                            StringComparison.InvariantCultureIgnoreCase)));
            }
            // Find Value (if languages not specified)
            else
                value = dbValues.Where(v => v.AttributeId == attribute.AttributeId)
                        .OrderBy(a => a.ChangeLogCreated)
                        .FirstOrDefault();

            if (value == null)
                value = AddSingleValue(entity, attribute.AttributeId, newValueSerialized);
            else
                // Update old/existing Value
                //if (value.ValueId != 0 || entity.EntityId <= 0) //  < 0 is ef-core temp id, 0 is other "not-defined" id
                //{
                // ro is always false
                //if (!readOnly)
                UpdateSingleValue(value, newValueSerialized);//, changeId);
            //}
            return value;
        }






        #endregion


    }
}
