﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public abstract class AppPermissionCheck: PermissionCheckBase
    {
        #region Constructor & DI
        protected AppPermissionCheck(string logName) : base(logName ?? "App.PrmChk") { }

        public AppPermissionCheck ForItem(IInstanceContext ctx, IAppIdentity appIdentity, IEntity targetItem, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, targetItem?.Type, targetItem);
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForType(IInstanceContext ctx, IAppIdentity appIdentity, IContentType targetType, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, targetType);
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForAttribute(IInstanceContext ctx, IAppIdentity appIdentity, IContentTypeAttribute attribute, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, permissions1: attribute.Permissions);
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForApp(IInstanceContext ctx, IApp app, ILog parentLog)
        {
            Init(ctx, app, parentLog, permissions1: app.Metadata.Permissions);
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForParts(IInstanceContext ctx, IApp app, IContentType targetType, IEntity targetItem, ILog parentLog)
        {
            Init(ctx, app, parentLog, targetType, targetItem, app.Metadata.Permissions);
            return Log.Call<AppPermissionCheck>()("ok", this);
        }


        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        protected AppPermissionCheck Init(
            IInstanceContext ctx,
            IAppIdentity appIdentity,
            ILog parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null, // optional entity to check
            IEnumerable<Permission> permissions1 = null
            //IEnumerable<Permission> permissions2 = null
            )
        {
            Init(parentLog, targetType ?? targetItem?.Type, targetItem, permissions1, null);
            var logWrap = Log.Call<AppPermissionCheck>($"..., {targetItem?.EntityId}, app: {appIdentity?.AppId}, ");
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            AppIdentity = appIdentity;
            return logWrap(null, this);
        }

        protected IInstanceContext Context { get; private set; }


        protected IAppIdentity AppIdentity;


        #endregion

        #region User Stuff

        /// <summary>
        /// Override base accessor for this
        /// </summary>
        protected override IUser User => Context.User;

        /// <summary>
        /// Check if user is super user
        /// </summary>
        /// <returns></returns>
        protected bool UserIsSuperuser() => Log.Intercept(nameof(UserIsSuperuser), () => Context.User?.IsSuperUser ?? false);

        /// <summary>
        /// Check if user is valid admin of current portal / zone
        /// </summary>
        /// <returns></returns>
        public bool UserIsTenantAdmin() => Log.Intercept(nameof(UserIsTenantAdmin), () => Context.User?.IsAdmin ?? false);
        // Portal?.UserInfo?.IsInRole(Portal?.AdministratorRoleName) ?? false);


        #endregion
    }
}
