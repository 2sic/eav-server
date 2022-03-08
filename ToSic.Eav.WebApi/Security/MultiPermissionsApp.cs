using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.Security
{
    /// <summary>
    /// Do consolidated permission checks on a set of permissions
    /// </summary>
    public class MultiPermissionsApp: MultiPermissionsBase
    {
        public class Dependencies
        {
            public Dependencies(IZoneMapper zoneMapper, Generator<AppPermissionCheck> appPermCheckGenerator)
            {
                ZoneMapper = zoneMapper;
                AppPermCheckGenerator = appPermCheckGenerator;
            }
            internal IZoneMapper ZoneMapper { get; }
            internal Generator<AppPermissionCheck> AppPermCheckGenerator { get; }
        }

        /// <summary>
        /// The current app which will be used and can be re-used externally
        /// </summary>
        protected IAppIdentity App { get; private set; }

        public IContextOfSite Context { get; private set; }

        protected ISite SiteForSecurityCheck { get; private set; }

        protected bool SamePortal { get; private set; }

        #region Constructors and DI

        public MultiPermissionsApp(Dependencies dependencies) : this(dependencies, "Api.Perms") { }

        protected MultiPermissionsApp(Dependencies dependencies, string logName) : base(logName ?? "Api.Perms")
        {
            _zoneMapper = dependencies.ZoneMapper.Init(Log);
            _appPermCheckGenerator = dependencies.AppPermCheckGenerator;
        }
        private readonly IZoneMapper _zoneMapper;
        private readonly Generator<AppPermissionCheck> _appPermCheckGenerator;

        public MultiPermissionsApp Init(IContextOfSite context, IAppIdentity app, ILog parentLog, string logName = null)
        {
            Init(parentLog, logName ?? "Api.PermApp");
            var wrapLog = Log.Call<MultiPermissionsApp>($"..., appId: {app.AppId}, ...");
            Context = context;
            App = app;

            SamePortal = context.Site.ZoneId == App.ZoneId;
            SiteForSecurityCheck = SamePortal 
                ? context.Site 
                // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
                // todo: probably enhance with a Site.IsMultiZone check
                : _zoneMapper.SiteOfZone(App.ZoneId) ?? context.Site;
            return wrapLog($"ready for z/a:{app.Show()} t/z:{SiteForSecurityCheck.Id}/{context.Site.ZoneId} same:{SamePortal}", this);
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
            var modifiedContext = Context.Clone(Log);
            modifiedContext.Site = SiteForSecurityCheck;
            return _appPermCheckGenerator.New /*Context.ServiceProvider.Build<AppPermissionCheck>()*/.ForParts(modifiedContext, App, type, item, Log);
        }

    }
}
