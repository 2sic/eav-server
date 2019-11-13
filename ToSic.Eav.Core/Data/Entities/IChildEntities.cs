using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This manages child entities - so entities referenced in a field of an Entity
    /// </summary>
    [PublicApi]
    public interface IChildEntities
    {
        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        /// <param name="attributeName">Attribute Name</param>
        /// <returns>List of Entities referenced in the mentioned field</returns>
        IEnumerable<IEntity> this[string attributeName] { get; }
    }
}