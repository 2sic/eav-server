using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A dictionary-style children accessor containing all fields which have child-entities. <br/>
    /// Used on the <see cref="IEntity"/> Children property.
    /// </summary>
    [PublicApi]
    public class Children : IChildren
    {
        private readonly Dictionary<string, IAttribute> _attributes;

        /// <summary>
        /// Initializes a new instance of the Children class.
        /// </summary>
        /// <param name="attributes"></param>
        internal Children(Dictionary<string, IAttribute> attributes)
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
                if (_attributes != null)
                {
                    if (_attributes.ContainsKey(attributeName))
                        return (_attributes[attributeName] as Attribute<IEnumerable<IEntity>>)?.TypedContents;
                    return new List<IEntity>();
                }
                // 2019-11-13 2dm - seems like the _objects was never used, so should always have been null
                return null;
            }
        }
    }
}
