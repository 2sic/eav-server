﻿using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DI;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Security
{
    /// <summary>
    /// Do consolidated permission checks on a set of permissions
    /// </summary>
    public class MultiPermissionsApp: MultiPermissionsBase
    {
        #region Constructors and DI

        public class Dependencies
        {
            public Dependencies(LazyInitLog<IZoneMapper> zoneMapper, Generator<AppPermissionCheck> appPermCheckGenerator, Generator<IFeaturesInternal> featIntGen)
            {
                ZoneMapper = zoneMapper;
                AppPermCheckGenerator = appPermCheckGenerator;
                FeatIntGen = featIntGen;
            }
            internal LazyInitLog<IZoneMapper> ZoneMapper { get; }
            internal Generator<AppPermissionCheck> AppPermCheckGenerator { get; }
            internal Generator<IFeaturesInternal> FeatIntGen { get; }

            internal Dependencies SetLog(ILog log)
            {
                ZoneMapper.SetLog(log);
                return this;
            }
        }


        public MultiPermissionsApp(Dependencies dependencies) : base("Api.Perms") => _dp = dependencies.SetLog(Log);
        private readonly Dependencies _dp;

        public MultiPermissionsApp Init(IContextOfSite context, IAppIdentity app, ILog parentLog, string logName = null)
        {
            this.Init(parentLog, logName ?? "Api.PermApp");
            var wrapLog = Log.Fn<MultiPermissionsApp>($"..., appId: {app.AppId}, ...");
            Context = context;
            App = app;

            SamePortal = context.Site.ZoneId == App.ZoneId;
            SiteForSecurityCheck = SamePortal 
                ? context.Site 
                // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
                // todo: probably enhance with a Site.IsMultiZone check
                : _dp.ZoneMapper.Value.SiteOfZone(App.ZoneId) ?? context.Site;
            return wrapLog.Return(this, $"ready for z/a:{app.Show()} t/z:{SiteForSecurityCheck.Id}/{context.Site.ZoneId} same:{SamePortal}");
        }
        /// <summary>
        /// The current app which will be used and can be re-used externally
        /// </summary>
        protected IAppIdentity App { get; private set; }
        public IContextOfSite Context { get; private set; }
        protected ISite SiteForSecurityCheck { get; private set; }
        protected bool SamePortal { get; private set; }
        public IFeaturesInternal FeaturesInternal => _dp.FeatIntGen.New();

        #endregion

        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => new Dictionary<string, IPermissionCheck> { { "App", BuildPermissionChecker() } };
        
        public bool ZoneIsOfCurrentContextOrUserIsSuper(out string error)
        {
            var wrapLog = Log.Fn<bool>();
            var zoneSameOrSuperUser = SamePortal || Context.User.IsSystemAdmin;
            error = zoneSameOrSuperUser ? null: $"accessing app {App.AppId} in zone {App.ZoneId} is not allowed for this user";
            return wrapLog.Return(zoneSameOrSuperUser, zoneSameOrSuperUser ? $"SamePortal:{SamePortal} - ok": "not ok, generate error");
        }



        /// <summary>
        /// Creates a permission checker for an app
        /// Optionally you can provide a type-name, which will be 
        /// included in the permission check
        /// </summary>
        /// <returns></returns>
        protected IPermissionCheck BuildPermissionChecker(IContentType type = null, IEntity item = null)
        {
            Log.A($"BuildPermissionChecker(type:{type?.Name}, item:{item?.EntityId})");

            // user has edit permissions on this app, and it's the same app as the user is coming from
            var modifiedContext = Context.Clone(Log);
            modifiedContext.Site = SiteForSecurityCheck;
            return _dp.AppPermCheckGenerator.New().ForParts(modifiedContext, App, type, item, Log);
        }

    }
}
