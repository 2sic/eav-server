using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironmentFactory
    {
        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// </summary>
        PermissionController ItemPermissions(IEntity targetItem, Log parentLog, IInstanceInfo module = null);

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of a type.
        /// </summary>
        PermissionController TypePermissions(IContentType targetType, IEntity targetItem, Log parentLog, IInstanceInfo module = null);
    }
}
