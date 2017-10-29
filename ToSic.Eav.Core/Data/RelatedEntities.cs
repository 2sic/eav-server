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
        private readonly Dictionary<string, object> _objects;

        /// <summary>
        /// Initializes a new instance of the ChildEntities class.
        /// </summary>
        /// <param name="attributes"></param>
        public RelatedEntities(Dictionary<string, IAttribute> attributes)
        {
            _attributes = attributes;
        }

        /// <summary>
        /// Initializes a new instance of the ChildEntities class.
        /// </summary>
        /// <param name="objects"></param>
        public RelatedEntities(Dictionary<string, object> objects)
        {
            _objects = objects;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        /// <param name="attributeName">Attribute Name</param>
        public IEnumerable<IEntity> this[string attributeName]
        {
            get
            {
                if (_attributes != null)
                {
                    if (_attributes.ContainsKey(attributeName))
                        return (_attributes[attributeName] as Attribute<EntityRelationship>)?.TypedContents;
                    return new List<IEntity>();
                }
                var objRelationships = _objects[attributeName] as EntityRelationship;
                return objRelationships;
            }
        }
    }
}
