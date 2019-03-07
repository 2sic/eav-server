using System.Collections.Generic;
using ToSic.Eav.Logging.Simple;

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

        /// <summary>
        /// WIP!
        /// </summary>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <param name="useNamedParameters"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        List<IEntity> FindChildren(string field = null, string type = null, string useNamedParameters = "xyz", Log log = null);

        /// <summary>
        /// WIP!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        List<IEntity> FindParents(string type = null, string field = null, string useNamedParameters = "xyz", Log log = null);
    }
}