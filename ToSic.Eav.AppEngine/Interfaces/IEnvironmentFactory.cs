﻿using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Environment;
using ToSic.Eav.Logging;
using ToSic.Eav.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;
using PermissionCheckBase = ToSic.Eav.Security.PermissionCheckBase;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public interface IEnvironmentFactory
    {
        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// </summary>
        PermissionCheckBase ItemPermissions(IInAppAndZone appIdentity, IEntity targetItem, ILog parentLog, IContainer module = null);

        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of a type.
        /// </summary>
        PermissionCheckBase TypePermissions(IInAppAndZone appIdentity, IContentType targetType, IEntity targetItem, ILog parentLog, IContainer module = null);

        /// <summary>
        /// Initialize to get permissions for an instance
        /// </summary>
        PermissionCheckBase InstancePermissions(ILog parentLog, IContainer module, IApp app);

        IPagePublishing PagePublisher(ILog parentLog);

        IAppEnvironment Environment(ILog parentLog);

    }
}
