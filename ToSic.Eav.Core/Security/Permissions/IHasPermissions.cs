using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Security.Permissions
{
    public interface IHasPermissions
    {
        /// <summary>
        /// List of permission entities for an item
        /// </summary>
        IEnumerable<IEntity> Permissions { get; }
    }
}