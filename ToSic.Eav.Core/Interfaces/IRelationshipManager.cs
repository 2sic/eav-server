using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IRelationshipManager
    {
        /// <summary>
        /// Get all Child Entities
        /// </summary>
        IEnumerable<IEntity> AllChildren { get; }

        /// <summary>
        /// Get all Parent Entities
        /// </summary>
        IEnumerable<IEntity> AllParents { get; }

        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        IRelatedEntities Children { get; }
    }
}