using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A dictionary-style children accessor containing all fields which have child-entities. <br/>
    /// Used on the <see cref="IEntity"/> Children property.
    /// </summary>
    [PrivateApi("this is for the Relationship.Children API, not recommended for others")]
    public class RelationshipChildren : IRelationshipChildren
    {
        private readonly IDictionary<string, IAttribute> _attributes;

        /// <summary>
        /// Initializes a new instance of the Children class.
        /// </summary>
        /// <param name="attributes"></param>
        internal RelationshipChildren(IDictionary<string, IAttribute> attributes)
        {
            _attributes = attributes;
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
                if (_attributes == null) return null;
                return _attributes.ContainsKey(attributeName) 
                    ? (_attributes[attributeName] as Attribute<IEnumerable<IEntity>>)?.TypedContents 
                    : new List<IEntity>();
            }
        }
    }
}
