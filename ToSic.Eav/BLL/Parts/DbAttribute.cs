using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace ToSic.Eav.BLL.Parts
{
    public class DbAttribute: BllCommandBase
    {
        public DbAttribute(EavDataController cntx) : base(cntx) {}

        /// <summary>
        /// Get Attributes of an AttributeSet
        /// </summary>
        public IQueryable<Attribute> GetAttributes(int attributeSetId)
        {
            attributeSetId = Context.AttribSet.ResolveAttributeSetId(attributeSetId);

            return from ais in Context.SqlDb.AttributesInSets
                   where ais.AttributeSetID == attributeSetId
                   orderby ais.SortOrder
                   select ais.Attribute;
        }


        /// <summary>
        /// Get a List of all Attributes in specified AttributeSet
        /// </summary>
        /// <param name="attributeSet">Reference to an AttributeSet</param>
        /// <param name="includeTitleAttribute">Specify whether TitleAttribute should be included</param>
        public List<Attribute> GetAttributes(AttributeSet attributeSet, bool includeTitleAttribute = true)
        {
            var items = Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == attributeSet.AttributeSetID);
            if (!includeTitleAttribute)
                items = items.Where(a => !a.IsTitle);

            return items.Select(a => a.Attribute).ToList();
        }

        /// <summary>
        /// Get Title Attribute for specified AttributeSetId
        /// </summary>
        public Attribute GetTitleAttribute(int attributeSetId)
        {
            return Context.SqlDb.AttributesInSets.Single(a => a.AttributeSetID == attributeSetId && a.IsTitle).Attribute;
        }


        /// <summary>
        /// Get a list of all Attributes in Set for specified AttributeSetId
        /// </summary>
        public List<AttributeInSet> GetAttributesInSet(int attributeSetId)
        {
            return Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).OrderBy(a => a.SortOrder).ToList();
        }

        ///// <summary>
        ///// Change the sort order of an attribute - move up or down
        ///// </summary>
        ///// <remarks>Does an interchange with the Sort Order below/above the current attribute</remarks>
        //public void ChangeAttributeOrder(int attributeId, int setId, AttributeMoveDirection direction)
        //{
        //    // todo 2dm: refactoring, causes some errors...
        //    var attributeList = Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == setId).ToList();

        //    var attributeToMove = attributeList.Single(a => a.AttributeID == attributeId);
        //    var attributeToInterchange = direction == AttributeMoveDirection.Up ?
        //        attributeList.OrderByDescending(a => a.SortOrder).First(a => a.SortOrder < attributeToMove.SortOrder) :
        //        attributeList.OrderBy(a => a.SortOrder).First(a => a.SortOrder > attributeToMove.SortOrder);

        //    var newSortOrder = attributeToInterchange.SortOrder;
        //    attributeToInterchange.SortOrder = attributeToMove.SortOrder;
        //    attributeToMove.SortOrder = newSortOrder;
        //    Context.SqlDb.SaveChanges();
        //}

        /// <summary>
        /// Update the order of the attributes in the set.
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="newSortOrder">Array of attribute ids which defines the new sort order</param>
        public void UpdateAttributeOrder(int setId, List<int> newSortOrder)
        {
            var attributeList = Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == setId).ToList();
            attributeList = attributeList.OrderBy(a => newSortOrder.IndexOf(a.AttributeID)).ToList();

            var index = 0;
            attributeList.ForEach(a => {
                a.SortOrder = index;
                index++;
            });

            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void SetTitleAttribute(int attributeId, int attributeSetId)
        {
            Context.SqlDb.AttributesInSets.Single(a => a.AttributeID == attributeId && a.AttributeSetID == attributeSetId).IsTitle = true;

            // unset other Attributes with isTitle=true
            var oldTitleAttributes = Context.SqlDb.AttributesInSets.Where(s => s.AttributeSetID == attributeSetId && s.IsTitle);
            foreach (var oldTitleAttribute in oldTitleAttributes)
                oldTitleAttribute.IsTitle = false;

            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void RenameStaticName(int attributeId, int attributeSetId, string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                throw new Exception("can't rename to something empty");

            // ensure that it's in the set
            var attr = Context.SqlDb.AttributesInSets.Single(a => a.AttributeID == attributeId && a.AttributeSetID == attributeSetId).Attribute;
            attr.StaticName = newName;
            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Update an Attribute
        /// </summary>
        public Attribute UpdateAttribute(int attributeId, string staticName)
        {
            return UpdateAttribute(attributeId, staticName, null);
        }
        /// <summary>
        /// Update an Attribute
        /// </summary>
        public Attribute UpdateAttribute(int attributeId, string staticName, int? attributeSetId = null, bool isTitle = false)
        {
            var attribute = Context.SqlDb.Attributes.Single(a => a.AttributeID == attributeId);
            Context.SqlDb.SaveChanges();

            if (isTitle)
                SetTitleAttribute(attributeId, attributeSetId.Value);

            return attribute;
        }


        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public Attribute AppendAttribute(AttributeSet attributeSet, string staticName, string type, string inputType, bool isTitle = false, bool autoSave = true)
        {
            return AppendAttribute(attributeSet, 0, staticName, type, inputType, isTitle, autoSave);
        }
        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public Attribute AppendAttribute(int attributeSetId, string staticName, string type, string inputType, bool isTitle = false)
        {
            return AppendAttribute(null, attributeSetId, staticName, type, inputType, isTitle, true);
        }
        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        private Attribute AppendAttribute(AttributeSet attributeSet, int attributeSetId, string staticName, string type, string inputType, bool isTitle, bool autoSave)
        {
            var sortOrder = attributeSet != null ? attributeSet.AttributesInSets.Max(s => (int?)s.SortOrder) : Context.SqlDb.AttributesInSets.Where(a => a.AttributeSetID == attributeSetId).Max(s => (int?)s.SortOrder);
            if (!sortOrder.HasValue)
                sortOrder = 0;
            else
                sortOrder++;

            return AddAttribute(attributeSet, attributeSetId, staticName, type, inputType, sortOrder.Value, 1, isTitle, autoSave);
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public Attribute AddAttribute(int attributeSetId, string staticName, string type, string inputType, int sortOrder = 0, int attributeGroupId = 1, bool isTitle = false, bool autoSave = true)
        {
            return AddAttribute(null, attributeSetId, staticName, type, inputType, sortOrder, attributeGroupId, isTitle, autoSave);
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        private Attribute AddAttribute(AttributeSet attributeSet, int attributeSetId, string staticName, string type, string inputType, int sortOrder, int attributeGroupId, bool isTitle, bool autoSave)
        {
            if (attributeSet == null)
                attributeSet = Context.SqlDb.AttributeSets.Single(a => a.AttributeSetID == attributeSetId);
            else if (attributeSetId != 0)
                throw new Exception("Can only set attributeSet or attributeSetId");

            if (!System.Text.RegularExpressions.Regex.IsMatch(staticName, Constants.AttributeStaticNameRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                throw new Exception("Attribute static name \"" + staticName + "\" is invalid. " + Constants.AttributeStaticNameRegExNotes);

            // Prevent Duplicate Name
            if (Context.SqlDb.AttributesInSets.Any(s => s.Attribute.StaticName == staticName && !s.Attribute.ChangeLogIDDeleted.HasValue && s.AttributeSetID == attributeSet.AttributeSetID && s.Set.AppID == Context.AppId /* _appId*/ ))
                throw new ArgumentException("An Attribute with static name " + staticName + " already exists", "staticName");

            var newAttribute = new Attribute
            {
                Type = type,
                StaticName = staticName,
                ChangeLogIDCreated = Context.Versioning.GetChangeLogId()
            };
            var setAssignment = new AttributeInSet
            {
                Attribute = newAttribute,
                Set = attributeSet,
                SortOrder = sortOrder,
                AttributeGroupID = attributeGroupId,
                IsTitle = isTitle
            };
            Context.SqlDb.AddToAttributes(newAttribute);
            Context.SqlDb.AddToAttributesInSets(setAssignment);

            // Set Attribute as Title if there's no title field in this set
            if (!attributeSet.AttributesInSets.Any(a => a.IsTitle))
                setAssignment.IsTitle = true;

            if (isTitle)
            {
                // unset old Title Fields
                var oldTitleFields = attributeSet.AttributesInSets.Where(a => a.IsTitle && a.Attribute.StaticName != staticName).ToList();
                foreach (var titleField in oldTitleFields)
                    titleField.IsTitle = false;
            }

            // If attribute has not been saved, we must save now to get the id (and assign entities)
            if (autoSave || newAttribute.AttributeID == 0)
                Context.SqlDb.SaveChanges();

            #region set the input type
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type "@All" to set the value to...
            var newValues = new Dictionary<string, object>
            {
                {"VisibleInEditUI", true },
                {"Name", staticName},
                {"InputType", inputType}
            };

            UpdateAttributeAdditionalProperties(newAttribute.AttributeID, true, newValues);
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
        public Entity UpdateAttributeAdditionalProperties(int attributeId, bool isAllProperty, IDictionary fieldProperties)
        {
            var fieldPropertyEntity = Context.SqlDb.Entities.FirstOrDefault(e => e.AssignmentObjectTypeID == Constants.AssignmentObjectTypeIdFieldProperties && e.KeyNumber == attributeId);
            if (fieldPropertyEntity != null)
                return Context.Entities.UpdateEntity(fieldPropertyEntity.EntityID, fieldProperties);

            var metaDataSetName = isAllProperty ? "@All" : "@" + Context.SqlDb.Attributes.Single(a => a.AttributeID == attributeId).Type;
            var systemScope = AttributeScope.System.ToString();
            var attSetFirst = Context.SqlDb.AttributeSets.FirstOrDefault(s => s.StaticName == metaDataSetName && s.Scope == systemScope && s.AppID == Context.AppId /* _appId*/);
            if(attSetFirst == null)
                throw new Exception("Can't continue, couldn't find attrib-set with: " + systemScope + ":" + metaDataSetName + " in app " + Context.AppId);
            var attributeSetId = attSetFirst.AttributeSetID;

            return Context.Entities.AddEntity(attributeSetId, fieldProperties, null, attributeId, Constants.AssignmentObjectTypeIdFieldProperties);
        }


        // todo: add security check if it really is in this app and content-type
        public bool RemoveAttribute(int attributeId)
        {
            // Remove values and valueDimensions of this attribute
            var values = Context.SqlDb.Values.Where(a => a.AttributeID == attributeId).ToList();
            values.ForEach(v => {
                v.ValuesDimensions.ToList().ForEach(vd => {
                    Context.SqlDb.ValuesDimensions.DeleteObject(vd);
                });
                Context.SqlDb.Values.DeleteObject(v);
            });
            Context.SqlDb.SaveChanges();

            var attr = Context.SqlDb.Attributes.FirstOrDefault(a => a.AttributeID == attributeId);

            if (attr != null)
                Context.SqlDb.Attributes.DeleteObject(attr);

            Context.SqlDb.SaveChanges();
            return true;
        }
    }
}
