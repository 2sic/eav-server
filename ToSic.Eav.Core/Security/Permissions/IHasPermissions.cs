using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Security
{
    /// <summary>
    /// Anything that uses <see cref="IHasPermissions"/> can have custom permissions.
    /// This interface provides access to the information stored in the custom permissions.
    /// </summary>
    [PublicApi]
    public interface IHasPermissions
    {
        /// <summary>
        /// Permissions are also stored as entity items. 
        /// </summary>
        /// <returns>
        /// List of permission entities for an item
        /// </returns>
        /// <remarks>
        /// Currently still a private API, because the type could change to be a typed permission object
        /// </remarks>
        IEnumerable<Permission> Permissions { get; }
    }
}