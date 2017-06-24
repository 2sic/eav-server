using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeDefinition
    {

        /// <summary>
        /// Get Attributes of an AttributeSet
        /// </summary>
        internal IQueryable<ToSicEavAttributes> GetAttributeDefinitions(int attributeSetId)
        {
            attributeSetId = DbContext.ContentType.ResolvePotentialGhostAttributeSetId(attributeSetId);

            return from ais in DbContext.SqlDb.ToSicEavAttributesInSets
                   where ais.AttributeSetId == attributeSetId
                   orderby ais.SortOrder
                   select ais.Attribute;
        }




        /// <summary>
        /// Get a list of all Attributes in Set for specified AttributeSetId
        /// </summary>
        public List<ToSicEavAttributesInSets> GetAttributesInSet(int attributeSetId)
        {
            return DbContext.SqlDb.ToSicEavAttributesInSets
                .Include(ais => ais.Attribute)
                .Where(a => a.AttributeSetId == attributeSetId)
                .OrderBy(a => a.SortOrder)
                .ToList();
        }


        /// <summary>
        /// Check if a valid, undeleted attribute-set exists
        /// </summary>
        /// <param name="attributeSetId"></param>
        /// <param name="staticName"></param>
        /// <returns></returns>
        internal bool AttributeExistsInSet(int attributeSetId, string staticName)
        {
            return DbContext.SqlDb.ToSicEavAttributesInSets.Any(s =>
                s.Attribute.StaticName == staticName
                && !s.Attribute.ChangeLogDeleted.HasValue
                && s.AttributeSetId == attributeSetId
                && s.AttributeSet.AppId == DbContext.AppId);
        }



        // new parts
        public string[] DataTypeNames(int appId)
        {
            return DbContext.SqlDb.ToSicEavAttributeTypes.OrderBy(a => a.Type).Select(a => a.Type).ToArray();
        }


    }
}
