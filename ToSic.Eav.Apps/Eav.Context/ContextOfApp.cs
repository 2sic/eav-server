﻿using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using static ToSic.Eav.Configuration.ConfigurationConstants;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    public class ContextOfApp: ContextOfSite, IContextOfApp
    {
        #region Constructor / DI

        /// <summary>
        /// These dependencies are a bit special, because they can be re-used for child context-of...
        /// This is why we gave them a much clearer name, not just the normal "Dependencies"
        /// </summary>
        public new class MyServices: MyServicesBase
        {
            public MyServices(
                IAppStates appStates,
                LazySvc<IFeaturesInternal> features,
                LazySvc<AppUserLanguageCheck> langChecks,
                Generator<IEnvironmentPermission> environmentPermissions,
                LazySvc<AppSettingsStack> settingsStack
            ) => ConnectServices(
                EnvironmentPermissions = environmentPermissions,
                AppStates = appStates,
                Features = features,
                LangChecks = langChecks,
                SettingsStack = settingsStack
            );

            public IAppStates AppStates { get; }
            public LazySvc<IFeaturesInternal> Features { get; }
            public LazySvc<AppUserLanguageCheck> LangChecks { get; }
            public LazySvc<AppSettingsStack> SettingsStack { get; }
            internal readonly Generator<IEnvironmentPermission> EnvironmentPermissions;
        }

        /// <summary>
        /// Constructor for DI
        /// </summary>
        /// <param name="siteCtxDeps"></param>
        /// <param name="services"></param>
        public ContextOfApp(ContextOfSite.MyServices siteCtxDeps, MyServices services) : this(siteCtxDeps, services, "Sxc.CtxApp")
        {
        }
        protected ContextOfApp(ContextOfSite.MyServices siteCtxDeps, MyServices services, string logName) : base(siteCtxDeps, logName)
        {
            Deps = services.SetLog(Log);
        }
        protected readonly MyServices Deps;

        #endregion

        public void ResetApp(IAppIdentity appIdentity) => Log.Do(() =>
        {
            if (AppIdentity == null || AppIdentity.AppId != appIdentity.AppId)
                AppIdentity = appIdentity;
        });

        public void ResetApp(int appId) => ResetApp(Deps.AppStates.IdentityOfApp(appId));

        protected virtual IAppIdentity AppIdentity
        {
            get => _appIdentity;
            set
            {
                _appIdentity = value;
                _appState.Reset();
                _appSettingsStack.Reset();
                _settings.Reset();
                _resources.Reset();
                _userMayEditGet.Reset();
            }
        }
        private IAppIdentity _appIdentity;

        public override bool UserMayEdit => _userMayEditGet.Get(() => Log.Getter(() =>
        {
            // Case 1: Superuser always may
            if (User.IsSystemAdmin) return (true, "super");

            // Case 2: No App-State
            if (AppState == null)
            {
                if (base.UserMayEdit) return (true, "no app, use default checks");

                // If user isn't allowed yet, it may be that the environment allows it
                var fromEnv = Deps.EnvironmentPermissions.New()
                    .Init(this as IContextOfSite, null)
                    .EnvironmentAllows(GrantSets.WriteSomething);

                return (fromEnv, "no app, result from Env");
            }

            // Case 3: From App
            var fromApp = SiteDeps.AppPermissionCheck.New()
                .ForAppInInstance(this, AppState)
                .UserMay(GrantSets.WriteSomething);

            // Check if language permissions may alter / remove edit permissions
            if (fromApp && Deps.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
                fromApp = Deps.LangChecks.Value.UserRestrictedByLanguagePermissions(AppState) ?? true;

            return (fromApp, $"{fromApp}");
        }));
        private readonly GetOnce<bool> _userMayEditGet = new GetOnce<bool>();

        public AppState AppState => _appState.Get(() => AppIdentity == null ? null : Deps.AppStates.Get(AppIdentity));
        private readonly GetOnce<AppState> _appState = new GetOnce<AppState>();

        #region Settings and Resources

        private AppSettingsStack AppSettingsStack => _appSettingsStack.Get(() => Deps.SettingsStack.Value.Init(AppState));
        private readonly GetOnce<AppSettingsStack> _appSettingsStack = new GetOnce<AppSettingsStack>();

        public PropertyStack AppSettings => _settings.Get(() => AppSettingsStack.GetStack(RootNameSettings));
        private readonly GetOnce<PropertyStack> _settings = new GetOnce<PropertyStack>();
        public PropertyStack AppResources => _resources.Get(() => AppSettingsStack.GetStack(RootNameResources));
        private readonly GetOnce<PropertyStack> _resources = new GetOnce<PropertyStack>();

        #endregion
    }
}
