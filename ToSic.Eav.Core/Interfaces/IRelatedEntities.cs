using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IRelatedEntities
    {
        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        /// <param name="attributeName">Attribute Name</param>
        IEnumerable<IEntity> this[string attributeName] { get; }
    }
}