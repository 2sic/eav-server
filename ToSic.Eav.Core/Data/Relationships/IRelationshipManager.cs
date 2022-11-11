using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Manages relationships of an entity - to it's children and parents.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
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
        [PrivateApi("not very useful for the outside, they should use FindChildren instead")]
        IRelationshipChildren Children { get; }

        /// <summary>
        /// Find the children with optional criteria.
        /// </summary>
        /// <param name="field">Get only the children of a specific field</param>
        /// <param name="type">Restrict the results to a specific ContentType</param>
        /// <param name="log">Optional logger, to debug what happens internally</param>
        /// <returns>Always returns a list - empty or containing results</returns>
        List<IEntity> FindChildren(string field = null, string type = null, ILog log = null);

        /// <summary>
        /// Find the parents with optional criteria.
        /// </summary>
        /// <param name="field">Get only the children of a specific field</param>
        /// <param name="type">Restrict the results to a specific ContentType</param>
        /// <param name="log">Optional logger, to debug what happens internally</param>
        /// <returns>Always returns a list - empty or containing results</returns>
        List<IEntity> FindParents(string type = null, string field = null, ILog log = null);

        [PrivateApi]
        IEnumerable<EntityRelationship> AllRelationships { get; }
    }
}