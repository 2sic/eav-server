using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
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
        public class ContextOfAppDependencies: DependenciesBase<ContextOfAppDependencies>
        {
            public ContextOfAppDependencies(
                IAppStates appStates,
                Lazy<IFeaturesInternal> featsLazy,
                LazyInitLog<AppUserLanguageCheck> langCheckLazy,
                GeneratorLog<IEnvironmentPermission> environmentPermissionGenerator,
                LazyInitLog<AppSettingsStack> settingsStack
            ) => AddToLogQueue(
                EnvironmentPermissionGenerator = environmentPermissionGenerator,
                AppStates = appStates,
                FeatsLazy = featsLazy,
                LangCheckLazy = langCheckLazy,
                SettingsStack = settingsStack
            );

            public IAppStates AppStates { get; }
            public Lazy<IFeaturesInternal> FeatsLazy { get; }
            public LazyInitLog<AppUserLanguageCheck> LangCheckLazy { get; }
            public LazyInitLog<AppSettingsStack> SettingsStack { get; }
            internal readonly GeneratorLog<IEnvironmentPermission> EnvironmentPermissionGenerator;
            //internal bool InitDone;
        }

        public ContextOfApp(ContextOfSiteDependencies contextOfSiteDependencies, ContextOfAppDependencies dependencies)
            : base(contextOfSiteDependencies)
        {
            Deps = dependencies;
            Deps.SetLog(Log);
            //if (!dependencies.InitDone)
            //{
            //    dependencies.LangCheckLazy.SetLog(Log);
            //    dependencies.EnvironmentPermissionGenerator.SetLog(Log);
            //    dependencies.InitDone = true;
            //}
            
            Log.Rename("Sxc.CtxApp");
        }
        protected readonly ContextOfAppDependencies Deps;

        #endregion

        public void ResetApp(IAppIdentity appIdentity)
        {
            if (AppIdentity == null || AppIdentity.AppId != appIdentity.AppId) 
                AppIdentity = appIdentity;
        }

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
                _userMayEdit = null;
            }
        }
        private IAppIdentity _appIdentity;

        public override bool UserMayEdit
        {
            get
            {
                if (_userMayEdit.HasValue) return _userMayEdit.Value;
                var wrapLog = Log.Fn<bool>();

                // Case 1: Superuser always may
                if (User.IsSystemAdmin)
                {
                    _userMayEdit = true;
                    return wrapLog.Return(_userMayEdit.Value, "super");
                }

                // Case 2: No App-State
                if (AppState == null)
                {
                    _userMayEdit = base.UserMayEdit;

                    if (_userMayEdit.Value)
                        return wrapLog.Return(_userMayEdit.Value, "no app, use fallback");

                    // If user isn't allowed yet, it may be that the environment allows it
                    _userMayEdit = Deps.EnvironmentPermissionGenerator.New()
                        .Init(this as IContextOfSite, null)
                        .EnvironmentAllows(GrantSets.WriteSomething);

                    return wrapLog.Return(_userMayEdit.Value, "no app, use fallback");
                }

                _userMayEdit = Dependencies.AppPermissionCheckGenerator.New()
                    .ForAppInInstance(this, AppState, Log)
                    .UserMay(GrantSets.WriteSomething);

                // Check if language permissions may alter edit
                if (_userMayEdit == true && Deps.FeatsLazy.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
                    _userMayEdit = Deps.LangCheckLazy.Value.UserRestrictedByLanguagePermissions(AppState) ?? _userMayEdit;

                return wrapLog.Return(_userMayEdit.Value, $"{_userMayEdit.Value}");
            }
        }
        private bool? _userMayEdit;

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
