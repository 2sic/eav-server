using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;
using PermissionCheckBase = ToSic.Eav.Security.PermissionCheckBase;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    [PrivateApi]
    public interface IEnvironmentFactory
    {
        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// </summary>
        PermissionCheckBase ItemPermissions(IAppIdentity appIdentity, IEntity targetItem, ILog parentLog, IContainer module = null);

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of a type.
        /// </summary>
        PermissionCheckBase TypePermissions(IAppIdentity appIdentity, IContentType targetType, IEntity targetItem, ILog parentLog, IContainer module = null);

        /// <summary>
        /// Initialize to get permissions for an instance
        /// </summary>
        PermissionCheckBase InstancePermissions(ILog parentLog, IContainer module, IApp app);

        IPagePublishing PagePublisher(ILog parentLog);

        IAppEnvironment Environment(ILog parentLog);

        // experimental
        IAppFileSystemLoader AppFileSystemLoader(int appId, string path, ILog log);
    }
}
