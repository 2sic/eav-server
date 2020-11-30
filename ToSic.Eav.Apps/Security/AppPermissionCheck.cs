using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    public abstract class AppPermissionCheck: PermissionCheckBase
    {
        #region Constructor & DI
        protected AppPermissionCheck(string logPrefix) : base($"{logPrefix}.PrmChk") { }

        public AppPermissionCheck ForItem(IContextOfSite ctx, IAppIdentity appIdentity, IEntity targetItem, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, targetItem?.Type, targetItem);
            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForType(IContextOfSite ctx, IAppIdentity appIdentity, IContentType targetType, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, targetType);
            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        public AppPermissionCheck ForAttribute(IContextOfSite ctx, IAppIdentity appIdentity, IContentTypeAttribute attribute, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, permissions: attribute.Permissions);
            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            return Log.Call<AppPermissionCheck>()("ok", this);
        }

        /// <summary>
        /// Init the check for an app.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="appIdentity">The App - in some cases (if no app exists yet) it's null</param>
        /// <param name="parentLog"></param>
        /// <returns></returns>
        public AppPermissionCheck ForAppInInstance(IContextOfSite ctx, IAppIdentity appIdentity, ILog parentLog)
        {
            var permissions = appIdentity == null
                ? null
                : (appIdentity as IApp)?.Metadata.Permissions.ToList()
                  ?? State.Get(appIdentity).AppMetadata.Permissions.ToList();
            Init(ctx, appIdentity, parentLog, permissions: permissions);

            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            var wrapLog = Log.Call<AppPermissionCheck>($"ctx, app: {appIdentity}, log");
            Log.Add($"Permissions: {permissions?.Count}");
            return wrapLog("ok", this);
        }

        public AppPermissionCheck ForParts(IContextOfSite ctx, IApp app, IContentType targetType, IEntity targetItem, ILog parentLog)
        {
            Init(ctx, app, parentLog, targetType, targetItem, app?.Metadata.Permissions);

            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            return Log.Call<AppPermissionCheck>()("ok", this);
        }


        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        protected AppPermissionCheck Init(
            IContextOfSite ctx,
            IAppIdentity appIdentity,
            ILog parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null, // optional entity to check
            IEnumerable<Permission> permissions = null
            )
        {
            Init(parentLog, targetType ?? targetItem?.Type, targetItem, permissions);
            var logWrap = Log.Call<AppPermissionCheck>($"..., {targetItem?.EntityId}, app: {appIdentity?.AppId}, ");
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            AppIdentity = appIdentity;
            return logWrap(null, this);
        }

        protected IContextOfSite Context { get; private set; }


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


        #endregion
    }
}
