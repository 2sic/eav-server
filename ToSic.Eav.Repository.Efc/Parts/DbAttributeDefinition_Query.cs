using System.Linq;
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
        /// Check if a valid, undeleted attribute-set exists
        /// </summary>
        /// <param name="attributeSetId"></param>
        /// <param name="staticName"></param>
        /// <returns></returns>
        internal bool AttributeExistsInSet(int attributeSetId, string staticName)
            => DbContext.SqlDb.ToSicEavAttributesInSets.Any(s =>
                s.Attribute.StaticName == staticName
                && !s.Attribute.ChangeLogDeleted.HasValue
                && s.AttributeSetId == attributeSetId
                && s.AttributeSet.AppId == DbContext.AppId);


        // new parts
        public string[] DataTypeNames(int appId)
            => DbContext.SqlDb.ToSicEavAttributeTypes.OrderBy(a => a.Type)
            .Select(a => a.Type)
            .ToArray();
    }
}
