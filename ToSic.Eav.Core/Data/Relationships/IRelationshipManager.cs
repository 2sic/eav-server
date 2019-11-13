using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Manages relationships of an entity - to it's children and parents.
    /// </summary>
    [PublicApi]
    public interface IRelationshipManager
    {
        /// <summary>
        /// Get all Child Entities
        /// </summary>
        /// <returns>List of all Entities referenced by this Entity.</returns>
        IEnumerable<IEntity> AllChildren { get; }

        /// <summary>
        /// Get all Parent Entities
        /// </summary>
        /// <returns>List of all Entities referencing this Entity.</returns>
        IEnumerable<IEntity> AllParents { get; }

        /// <summary>
        /// Get Children of a specified Attribute Name
        /// </summary>
        IChildEntities Children { get; }

        /// <summary>
        /// WIP!
        /// </summary>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <param name="useNamedParameters"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [PrivateApi]
        List<IEntity> FindChildren(string field = null, string type = null, string useNamedParameters = "xyz", ILog log = null);

        /// <summary>
        /// WIP!
        /// </summary>
        /// <param name="type"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        [PrivateApi]
        List<IEntity> FindParents(string type = null, string field = null, string useNamedParameters = "xyz", ILog log = null);
    }
}