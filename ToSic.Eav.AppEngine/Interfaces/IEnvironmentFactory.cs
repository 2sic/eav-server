using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironmentFactory
    {
        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// </summary>
        PermissionCheckBase ItemPermissions(IAppIdentity appIdentity, IEntity targetItem, ILog parentLog, IInstanceInfo module = null);

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of a type.
        /// </summary>
        PermissionCheckBase TypePermissions(IAppIdentity appIdentity, IContentType targetType, IEntity targetItem, ILog parentLog, IInstanceInfo module = null);

        /// <summary>
        /// Initialize to get permissions for an instance
        /// </summary>
        PermissionCheckBase InstancePermissions(ILog parentLog, IInstanceInfo module, IApp app);

        IPagePublishing PagePublisher(ILog parentLog);

        IAppEnvironment Environment(ILog parentLog);

    }
}
