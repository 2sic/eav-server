﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Security.Permissions
{
    /// <summary>
    /// Anything that uses <see cref="IHasPermissions"/> can have custom permissions.
    /// This interface provides access to the information stored in the custom permissions.
    /// </summary>
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
        [PrivateApi]
        IEnumerable<IEntity> Permissions { get; }
    }
}