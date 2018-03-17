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
        PermissionCheckBase ItemPermissions(IEntity targetItem, Log parentLog, IInstanceInfo module = null);

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of a type.
        /// </summary>
        PermissionCheckBase TypePermissions(IContentType targetType, IEntity targetItem, Log parentLog, IInstanceInfo module = null);

        /// <summary>
        /// Initialize to get permissions for an instance
        /// </summary>
        PermissionCheckBase InstancePermissions(Log parentLog, IInstanceInfo module, IApp app);

        IPagePublishing PagePublisher(Log parentLog);

        IEnvironment Environment(Log parentLog);
    }
}
