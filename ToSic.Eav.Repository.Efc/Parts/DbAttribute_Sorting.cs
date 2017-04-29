using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttribute
    {

        /// <summary>
        /// Update the order of the attributes in the set.
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="newSortOrder">Array of attribute ids which defines the new sort order</param>
        internal void UpdateAttributeOrder(int setId, List<int> newSortOrder)
        {
            var attributeList = DbContext.SqlDb.ToSicEavAttributesInSets
                .Where(a => a.AttributeSetId == setId)
                .ToList();
            attributeList = attributeList
                .OrderBy(a => newSortOrder.IndexOf(a.AttributeId))
                .ToList();

            PersistAttributeOrder(attributeList);
        }

        internal void PersistAttributeOrder(List<ToSicEavAttributesInSets> attributeList)
        {
            var index = 0;
            DbContext.DoAndSave(() => attributeList.ForEach(a => a.SortOrder = index++));
            //attributeList.ForEach(a => a.SortOrder = index++);
            //DbContext.SqlDb.SaveChanges();
        }
    }
}
