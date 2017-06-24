using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeDefinition: BllCommandBase
    {
        public DbAttributeDefinition(DbDataController cntx) : base(cntx) {}

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void SetTitleAttribute(int attributeId, int attributeSetId)
        {
            DbContext.SqlDb.ToSicEavAttributesInSets
                .Single(a => a.AttributeId == attributeId && a.AttributeSetId == attributeSetId).IsTitle = true;

            // unset other Attributes with isTitle=true
            var oldTitleAttributes = DbContext.SqlDb.ToSicEavAttributesInSets
                .Where(s => s.AttributeSetId == attributeSetId && s.IsTitle);
            foreach (var oldTitleAttribute in oldTitleAttributes)
                oldTitleAttribute.IsTitle = false;

            DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Set an Attribute as Title on an AttributeSet
        /// </summary>
        public void RenameAttribute(int attributeId, int attributeSetId, string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                throw new Exception("can't rename to something empty");

            // ensure that it's in the set
            var attr = DbContext.SqlDb.ToSicEavAttributesInSets
                .Include(a => a.Attribute)
                .Single(a => a.AttributeId == attributeId && a.AttributeSetId == attributeSetId)
                .Attribute;
            attr.StaticName = newName;
            DbContext.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        internal int AppendToEndAndSave(ToSicEavAttributeSets attributeSet, int attributeSetId, string staticName, string type, bool isTitle)
        {
            var maxIndex = attributeSet != null
                ? attributeSet.ToSicEavAttributesInSets
                    .Max(s => (int?) s.SortOrder)
                : DbContext.SqlDb.ToSicEavAttributesInSets
                    .Where(a => a.AttributeSetId == attributeSetId)
                    .Max(s => (int?) s.SortOrder);

            maxIndex = !maxIndex.HasValue ? 0 : maxIndex + 1;

            return AddAttributeAndSave(attributeSet, attributeSetId, staticName, type, maxIndex.Value, 1, isTitle);
        }
        
        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// </summary>
        public int AddAttributeAndSave(ToSicEavAttributeSets attributeSet, int attributeSetId, string staticName, string type, /*string inputType,*/ int sortOrder, int attributeGroupId, bool isTitle)//, bool autoSave)
        {
            if (attributeSet == null)
                attributeSet = DbContext.SqlDb.ToSicEavAttributeSets
                    .Single(a => a.AttributeSetId == attributeSetId);
            else if (attributeSetId != 0)
                throw new Exception("Can only set attributeSet or attributeSetId");

            if (!Constants.AttributeStaticName.IsMatch(staticName))
                throw new Exception("Attribute static name \"" + staticName + "\" is invalid. " + Constants.AttributeStaticNameRegExNotes);

            // Prevent Duplicate Name
            if (AttributeExistsInSet(attributeSet.AttributeSetId, staticName))
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

            DbContext.SqlDb.SaveChanges();
            return newAttribute.AttributeId;
        }
        


        public bool RemoveAttributeAndAllValuesAndSave(int attributeId)
        {
            // Remove values and valueDimensions of this attribute
            var values = DbContext.SqlDb.ToSicEavValues
                .Where(a => a.AttributeId == attributeId).ToList();

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



    }
}
