using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Security;

namespace ToSic.Eav.Apps.Security
{
    /// <summary>
    /// Check permissions on something inside an App, like a specific Entity, Content-Type etc.
    /// </summary>
    public class AppPermissionCheck: PermissionCheckBase
    {
        #region Constructor & DI
        public AppPermissionCheck(IAppStates appStates, Dependencies dependencies) : base(dependencies, $"{AppConstants.LogName}.PrmChk")
        {
            _appStates = appStates;
            _environmentPermission = (EnvironmentPermission) dependencies.EnvironmentPermission.Init(Log);
        }
        private readonly IAppStates _appStates;
        private readonly EnvironmentPermission _environmentPermission;

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

        public AppPermissionCheck ForCustom(IContextOfSite ctx, IAppIdentity appIdentity, IEnumerable<Permission> permissions, ILog parentLog)
        {
            Init(ctx, appIdentity, parentLog, permissions: permissions);
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
            var permissions = FindPermissionsOfApp(appIdentity);
            Init(ctx, appIdentity, parentLog, permissions: permissions);
            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            var wrapLog = Log.Call<AppPermissionCheck>($"ctx, app: {appIdentity}, log");
            Log.Add($"Permissions: {permissions?.Count}");
            return wrapLog("ok", this);
        }

        private List<Permission> FindPermissionsOfApp(IAppIdentity appIdentity)
        {
            var permissions = appIdentity == null
                ? null
                : (appIdentity as IApp)?.Metadata.Permissions.ToList()
                  ?? (appIdentity as AppState)?.Metadata.Permissions.ToList()
                  ??  _appStates.Get(appIdentity).Metadata.Permissions.ToList();
            return permissions;
        }

        public AppPermissionCheck ForParts(IContextOfSite ctx, IAppIdentity app, IContentType targetType, IEntity targetItem, ILog parentLog)
        {
            Init(ctx, app, parentLog, targetType, targetItem, FindPermissionsOfApp(app));

            // note: WrapLog shouldn't be created before the init, because otherwise we don't see the results
            return Log.Call<AppPermissionCheck>()("ok", this);
        }


        /// <summary>
        /// Initialize this object so it can then give information regarding the permissions of an entity.
        /// Uses a GUID as identifier because that survives export/import. 
        /// </summary>
        private void Init(
            IContextOfSite ctx,
            IAppIdentity appIdentity,
            ILog parentLog,
            IContentType targetType = null, // optional type to check
            IEntity targetItem = null, // optional entity to check
            IEnumerable<Permission> permissions = null)
        {
            Init(parentLog, targetType ?? targetItem?.Type, targetItem, permissions);
            _environmentPermission.Init(ctx, appIdentity);
            var logWrap = Log.Call($"..., {targetItem?.EntityId}, app: {appIdentity?.AppId}, ");
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            AppIdentity = appIdentity;
            logWrap(null);
        }

        protected IContextOfSite Context { get; private set; }


        protected IAppIdentity AppIdentity;


        #endregion

        #region User Stuff

        /// <summary>
        /// Override base accessor for this
        /// </summary>
        protected override IUser User => Context.User;

        #endregion
    }
}
