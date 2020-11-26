using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using IApp = ToSic.Eav.Apps.IApp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.Security
{
    public class MultiPermissionsApp: MultiPermissionsBase
    {
        private readonly IZoneMapper _zoneMapper;

        /// <summary>
        /// The current app which will be used and can be re-used externally
        /// </summary>
        public IApp App { get; private set; }

        internal IRunContextCore Context { get; private set; }

        protected ISite SiteForSecurityCheck { get; private set; }

        protected bool SamePortal { get; private set; }

        #region Constructors and DI

        public MultiPermissionsApp(IZoneMapper zoneMapper) : this(zoneMapper, "Api.Perms") { }

        protected MultiPermissionsApp(IZoneMapper zoneMapper, string logName) : base(logName ?? "Api.Perms")
        {
            _zoneMapper = zoneMapper;
            _zoneMapper.Init(Log);
        }

        public MultiPermissionsApp Init(IContextOfBlock context, IApp app, ILog parentLog, string logName = null)
        {
            Init(parentLog, logName ?? "Api.PermApp");
            var wrapLog = Log.Call<MultiPermissionsApp>($"..., appId: {app.AppId}, ...");
            Context = context;
            App = app;

            SamePortal = Context.Site.ZoneId == App.ZoneId;
            SiteForSecurityCheck = SamePortal ? Context.Site : _zoneMapper.SiteOfZone(App.ZoneId);
            return wrapLog($"ready for z/a:{app.Show()} t/z:{App.Site.Id}/{Context.Site.ZoneId} same:{SamePortal}", this);
        }

        #endregion

        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => new Dictionary<string, IPermissionCheck>
            {
                {"App", BuildPermissionChecker()}
            };

        public bool ZoneIsOfCurrentContextOrUserIsSuper(out string error)
        {
            var wrapLog = Log.Call<bool>();
            var zoneSameOrSuperUser = SamePortal || Context.User.IsSuperUser;
            error = zoneSameOrSuperUser ? null: $"accessing app {App.AppId} in zone {App.ZoneId} is not allowed for this user";
            return wrapLog(zoneSameOrSuperUser ? $"SamePortal:{SamePortal} - ok": "not ok, generate error", zoneSameOrSuperUser);
        }



        /// <summary>
        /// Creates a permission checker for an app
        /// Optionally you can provide a type-name, which will be 
        /// included in the permission check
        /// </summary>
        /// <returns></returns>
        protected IPermissionCheck BuildPermissionChecker(IContentType type = null, IEntity item = null)
        {
            Log.Add($"BuildPermissionChecker(type:{type?.Name}, item:{item?.EntityId})");

            // user has edit permissions on this app, and it's the same app as the user is coming from
            var modifiedContext = Context.Clone();
            modifiedContext.Site = SiteForSecurityCheck;
            return Context.ServiceProvider.Build<AppPermissionCheck>().ForParts(modifiedContext, App, type, item, Log);
        }

    }
}
