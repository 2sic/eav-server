using System.Linq;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbAttributeSet
    {

        /// <summary>
        /// if AttributeSet refers another AttributeSet, get ID of the refered AttributeSet. Otherwise returns passed AttributeSetId.
        /// </summary>
        /// <param name="attributeSetId">AttributeSetId to resolve</param>
        internal int ResolvePotentialGhostAttributeSetId(int attributeSetId)
        {
            var usesConfigurationOfAttributeSet = DbContext.SqlDb.ToSicEavAttributeSets
                .Where(a => a.AttributeSetId == attributeSetId)
                .Select(a => a.UsesConfigurationOfAttributeSet)
                .Single();
            return usesConfigurationOfAttributeSet ?? attributeSetId;
        }
    }
}
