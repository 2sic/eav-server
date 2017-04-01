using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents Child Entities by attribute name
    /// </summary>
    public class RelatedEntities : IRelatedEntities
    {
        private readonly Dictionary<string, IAttribute> _attributes;

        /// <summary>
        /// Initializes a new instance of the ChildEntities class.
        /// </summary>
        /// <param name="attributes"></param>
        public RelatedEntities(Dictionary<string, IAttribute> attributes)
        {
            _attributes = attributes;
        }

        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        /// <param name="attributeName">Attribute Name</param>
        public IEnumerable<IEntity> this[string attributeName]
        {
            get
            {
                Attribute<Data.EntityRelationship> relationship;
                try
                {
                    relationship = _attributes[attributeName] as Attribute<Data.EntityRelationship>;
                }
                catch (KeyNotFoundException)
                {
                    return new List<IEntity>();
                }

                return relationship?.TypedContents;
            }
        }
    }
}
