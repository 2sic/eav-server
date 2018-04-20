using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Security.Permissions
{
    public interface IHasPermissions
    {
        IEnumerable<IEntity> Permissions { get; }
    }
}