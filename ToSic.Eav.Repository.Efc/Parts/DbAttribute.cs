using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbAttribute: BllCommandBase
    {
        public DbAttribute(DbDataController cntx) : base(cntx) {}

        /// <summary>
        /// Get Attributes of an AttributeSet
        /// </summary>
        public IQueryable<ToSicEavAttributes> GetAttributeDefinitions(int attributeSetId)
        {
            attributeSetId = DbContext.AttribSet.ResolveAttributeSetId(attributeSetId);

            return from ais in DbContext.SqlDb.ToSicEavAttributesInSets
                   where ais.AttributeSetId == attributeSetId
                   orderby ais.SortOrder
                   select ais.Attribute;
        }


        ///// <summary>
        ///// Get a List of all Attributes in specified AttributeSet
        ///// </summary>
        ///// <param name="attributeSet">Reference to an AttributeSet</param>
        ///// <param name="includeTitleAttribute">Specify whether TitleAttribute should be included</param>
        //public List<Attribute> GetAttributes(AttributeSet attributeSet, bool includeTitleAttribute = true)
        //{
        //    var items = Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetId == attributeSet.AttributeSetId);
        //    if (!includeTitleAttribute)
        //        items = items.Where(a => !a.IsTitle);

        //    return items.Select(a => a.Attribute).ToList();
        //}
        //
        ///// <summary>
        ///// Get Title Attribute for specified AttributeSetId
        ///// </summary>
        //public Attribute GetTitleAttribute(int attributeSetId)
        //{
        //    return Context.SqlDb.AttributesInSets.Single(a => a.AttributeSetId == attributeSetId && a.IsTitle).Attribute;
        //}


        /// <summary>
        /// Get a list of all Attributes in Set for specified AttributeSetId
        /// </summary>
        public List<ToSicEavAttributesInSets> GetAttributesInSet(int attributeSetId)
        {
            return DbContext.SqlDb.ToSicEavAttributesInSets.Where(a => a.AttributeSetId == attributeSetId).OrderBy(a => a.SortOrder).ToList();
        }

        /// <summary>
        /// Update the order of the attributes in the set.
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="newSortOrder">Array of attribute ids which defines the new sort order</param>
        public void UpdateAttributeOrder(int setId, List<int> newSortOrder)
        {
            var attributeList = DbContext.SqlDb.ToSicEavAttributesInSets.Where(a => a.AttributeSetId == setId).ToList();
            attributeList = attributeList.OrderBy(a => newSortOrder.IndexOf(a.AttributeId)).ToList();

            PersistAttributeSorting(attributeList);
        }

        public void PersistAttributeSorting(List<ToSicEavAttributesInSets> attributeList)
        {
            var index = 0;
            attributeList.ForEach(a => a.SortOrder = index++);
            DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void SetTitleAttribute(int attributeId, int attributeSetId)
        {
            DbContext.SqlDb.ToSicEavAttributesInSets.Single(a => a.AttributeId == attributeId && a.AttributeSetId == attributeSetId).IsTitle = true;

            // unset other Attributes with isTitle=true
            var oldTitleAttributes = DbContext.SqlDb.ToSicEavAttributesInSets.Where(s => s.AttributeSetId == attributeSetId && s.IsTitle);
            foreach (var oldTitleAttribute in oldTitleAttributes)
                oldTitleAttribute.IsTitle = false;

            DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void RenameStaticName(int attributeId, int attributeSetId, string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                throw new Exception("can't rename to something empty");

            // ensure that it's in the set
            var attr = DbContext.SqlDb.ToSicEavAttributesInSets.Single(a => a.AttributeId == attributeId && a.AttributeSetId == attributeSetId).Attribute;
            attr.StaticName = newName;
            DbContext.SqlDb.SaveChanges();
        }

        ///// <summary>
        ///// Update an Attribute
        ///// </summary>
        //public Attribute UpdateAttribute(int attributeId, string staticName)
        //{
        //    return UpdateAttribute(attributeId, staticName, null);
        //}
        ///// <summary>
        ///// Update an Attribute
        ///// </summary>
        //public Attribute UpdateAttribute(int attributeId, string staticName, int? attributeSetId = null, bool isTitle = false)
        //{
        //    var attribute = Context.SqlDb.Attributes.Single(a => a.AttributeId == attributeId);
        //    Context.SqlDb.SaveChanges();

        //    if (isTitle)
        //        SetTitleAttribute(attributeId, attributeSetId.Value);

        //    return attribute;
        //}


        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public ToSicEavAttributes AppendAttribute(ToSicEavAttributeSets attributeSet, string staticName, string type, string inputType, bool isTitle = false, bool autoSave = true)
        {
            return AppendAttribute(attributeSet, 0, staticName, type, inputType, isTitle, autoSave);
        }
        ///// <summary>
        ///// Append a new Attribute to an AttributeSet
        ///// </summary>
        //public Attribute AppendAttribute(int attributeSetId, string staticName, string type, string inputType, bool isTitle = false)
        //{
        //    return AppendAttribute(null, attributeSetId, staticName, type, inputType, isTitle, true);
        //}
        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        private ToSicEavAttributes AppendAttribute(ToSicEavAttributeSets attributeSet, int attributeSetId, string staticName, string type, string inputType, bool isTitle, bool autoSave)
        {
            var sortOrder = attributeSet != null ? attributeSet.ToSicEavAttributesInSets.Max(s => (int?)s.SortOrder) : DbContext.SqlDb.ToSicEavAttributesInSets.Where(a => a.AttributeSetId == attributeSetId).Max(s => (int?)s.SortOrder);
            if (!sortOrder.HasValue)
                sortOrder = 0;
            else
                sortOrder++;

            return AddAttribute(attributeSet, attributeSetId, staticName, type, inputType, sortOrder.Value, 1, isTitle, autoSave);
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public /*ToSicEavAttributes*/ int AddAttribute(int attributeSetId, string staticName, string type, string inputType, int sortOrder = 0, int attributeGroupId = 1, bool isTitle = false, bool autoSave = true)
        {
            return AddAttribute(null, attributeSetId, staticName, type, inputType, sortOrder, attributeGroupId, isTitle, autoSave)
                .AttributeId;
        }


        internal bool Exists(int attributeSetId, string staticName)
        {
            return DbContext.SqlDb.ToSicEavAttributesInSets.Any(
                s =>
                    s.Attribute.StaticName == staticName && !s.Attribute.ChangeLogDeleted.HasValue &&
                    s.AttributeSetId == attributeSetId && s.AttributeSet.AppId == DbContext.AppId);
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        private ToSicEavAttributes AddAttribute(ToSicEavAttributeSets attributeSet, int attributeSetId, string staticName, string type, string inputType, int sortOrder, int attributeGroupId, bool isTitle, bool autoSave)
        {
            if (attributeSet == null)
                attributeSet = DbContext.SqlDb.ToSicEavAttributeSets.Single(a => a.AttributeSetId == attributeSetId);
            else if (attributeSetId != 0)
                throw new Exception("Can only set attributeSet or attributeSetId");

//            if (!System.Text.RegularExpressions.Regex.IsMatch(staticName, Constants.AttributeStaticNameRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            if (!Constants.AttributeStaticName.IsMatch(staticName))
                throw new Exception("Attribute static name \"" + staticName + "\" is invalid. " + Constants.AttributeStaticNameRegExNotes);

            // Prevent Duplicate Name
            if (Exists(attributeSet.AttributeSetId, staticName))// Context.SqlDb.AttributesInSets.Any(s => s.Attribute.StaticName == staticName && !s.Attribute.ChangeLogDeleted.HasValue && s.AttributeSetId == attributeSet.AttributeSetId && s.Set.AppId == Context.AppId ))
                throw new ArgumentException("An Attribute with static name " + staticName + " already exists", nameof(staticName));

            var newAttribute = new ToSicEavAttributes
            {
                Type = type,
                StaticName = staticName,
                ChangeLogCreated = DbContext.Versioning.GetChangeLogId()
            };
            var setAssignment = new ToSicEavAttributesInSets
            {
                Attribute = newAttribute,
                AttributeSet = attributeSet,
                SortOrder = sortOrder,
                AttributeGroupId = attributeGroupId,
                IsTitle = isTitle
            };
            DbContext.SqlDb.Add(newAttribute);
            DbContext.SqlDb.Add(setAssignment);

            // Set Attribute as Title if there's no title field in this set
            if (!attributeSet.ToSicEavAttributesInSets.Any(a => a.IsTitle))
                setAssignment.IsTitle = true;

            if (isTitle)
            {
                // unset old Title Fields
                var oldTitleFields = attributeSet.ToSicEavAttributesInSets.Where(a => a.IsTitle && a.Attribute.StaticName != staticName).ToList();
                foreach (var titleField in oldTitleFields)
                    titleField.IsTitle = false;
            }

            // If attribute has not been saved, we must save now to get the id (and assign entities)
            if (autoSave || newAttribute.AttributeId == 0)
                DbContext.SqlDb.SaveChanges();

            #region set the input type
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type "@All" to set the value to...
            var newValues = new Dictionary<string, object>
            {
                {"VisibleInEditUI", true },
                {"Name", staticName},
                {"InputType", inputType}
            };

            UpdateAttributeAdditionalProperties(newAttribute.AttributeId, true, newValues);
            #endregion

            return newAttribute;
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            var newValues = new Dictionary<string, object> {
                { "InputType", inputType }
            };

            UpdateAttributeAdditionalProperties(attributeId, true, newValues);
            return true;
        }

        /// <summary>
        /// Update AdditionalProperties of an attribute 
        /// </summary>
        public ToSicEavEntities UpdateAttributeAdditionalProperties(int attributeId, bool isAllProperty, IDictionary fieldProperties)
        {
            var fieldPropertyEntity = DbContext.SqlDb.ToSicEavEntities.FirstOrDefault(e => e.AssignmentObjectTypeId == Constants.MetadataForField && e.KeyNumber == attributeId);
            if (fieldPropertyEntity != null)
                return DbContext.Entities.UpdateEntity(fieldPropertyEntity.EntityId, fieldProperties/*, masterRecord:true*/);

            var metaDataSetName = isAllProperty ? "@All" : "@" + DbContext.SqlDb.ToSicEavAttributes.Single(a => a.AttributeId == attributeId).Type;
            var systemScope = AttributeScope.System.ToString();
            var attSetFirst = DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(s => s.StaticName == metaDataSetName && s.Scope == systemScope && s.AppId == DbContext.AppId && !s.ChangeLogDeleted.HasValue /* _appId*/);
            if(attSetFirst == null)
                throw new Exception("Can't continue, couldn't find attrib-set with: " + systemScope + ":" + metaDataSetName + " in app " + DbContext.AppId);
            var attributeSetId = attSetFirst.AttributeSetId;

            return DbContext.Entities.AddEntity(attributeSetId, fieldProperties, keyNumber: attributeId, keyTypeId: Constants.MetadataForField);
        }


        // todo: add security check if it really is in this app and content-type
        public bool RemoveAttribute(int attributeId)
        {
            // Remove values and valueDimensions of this attribute
            var values = DbContext.SqlDb.ToSicEavValues.Where(a => a.AttributeId == attributeId).ToList();
            values.ForEach(v => {
                v.ToSicEavValuesDimensions.ToList().ForEach(vd => {
                    DbContext.SqlDb.ToSicEavValuesDimensions.Remove(vd);
                });
                DbContext.SqlDb.ToSicEavValues.Remove(v);
            });
            DbContext.SqlDb.SaveChanges();

            var attr = DbContext.SqlDb.ToSicEavAttributes.FirstOrDefault(a => a.AttributeId == attributeId);

            if (attr != null)
                DbContext.SqlDb.ToSicEavAttributes.Remove(attr);

            DbContext.SqlDb.SaveChanges();
            return true;
        }


        // new parts
        public string[] DataTypes(int appId)
        {
            //SetAppIdAndUser(appId);
            return DbContext.SqlDb.ToSicEavAttributeTypes.OrderBy(a => a.Type).Select(a => a.Type).ToArray();
        }

    }
}
