using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static System.StringComparison;
using static ToSic.Eav.Configuration.ConfigurationConstants;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This object creates caches for Settings or Resources
    /// It will handle all kinds of thing-lists incl. SystemSettings / SystemResources, Settings, Resources etc.
    /// </summary>
    [PrivateApi]
    public class AppStateStackCache
    {
        public readonly AppThingsIdentifiers Target;

        private AppState Parent { get; }
        private AppState Site { get; }
        private AppState Global { get; }

        internal AppStateStackCache(AppState parent, AppState site, AppState global, AppThingsIdentifiers target)
        {
            Target = target;
            Parent = parent;
            Site = site; // appStates.Get(appStates.Identity(Parent.ZoneId, null));
            Global = global; // appStates.Get(appStates.Identity(null, null));
        }

        /// <summary>
        /// The App-Settings or App-Resources
        /// </summary>
        public IEntity MetadataItem => (_appItemSynced ?? (_appItemSynced = AppStateSettings.BuildSynchedMetadata(Parent, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;

        /// <summary>
        /// The SystemSettings/SystemResources on this App which are scoped to this specific App.
        /// Used on the main App and on the Global-App (for global settings, which are automatically global)
        /// </summary>
        public IEntity ForApp => SysEntitiesInApp.List
            .FirstOrDefault(e => string.IsNullOrEmpty(e.GetBestValue<string>(SysSettingsFieldScope, null)));

        /// <summary>
        /// The SystemSettings/SystemResources on this App which are scoped to this entire Site.
        /// Only relevant on the primary content-app or on the global-app
        /// </summary>
        public IEntity ForSite => SysEntitiesInApp.List
            .FirstOrDefault(e => SysSettingsScopeValueSite.Equals(e.GetBestValue<string>(SysSettingsFieldScope, null), InvariantCultureIgnoreCase));


        /// <summary>
        /// The custom settings - usually a type "Settings" or "Resources" which is on the site-app or global-app
        /// </summary>
        public IEntity Custom
            => (_siteSettingsCustom ?? (_siteSettingsCustom = MakeSyncListOfType(Target.CustomType)))
                .List
                .FirstOrDefault();
        private SynchronizedEntityList _siteSettingsCustom;

        #region Full Stack

        /// <summary>
        /// Get the stack of Settings which applies to this app
        /// </summary>
        /// <returns></returns>
        internal List<KeyValuePair<string, IPropertyLookup>> FullStack(IServiceProvider sp)
            => (_fullStackSynched ?? (_fullStackSynched = BuildCachedStack(sp))).Value;
        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _fullStackSynched;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack(IServiceProvider sp)
        {
            //var primaryAppState = _appStates.Get(_appStates.Identity(Parent.ZoneId, null));
            //var globalAppState = _appStates.Get(_appStates.Identity(null, null));

            var cachedStack =
                new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(
                    new CacheExpiringMultiSource(Parent, Site, Global),
                    () => RebuildStack(sp, Site, Global));
            return cachedStack;
        }


        private List<KeyValuePair<string, IPropertyLookup>> RebuildStack(IServiceProvider sp, AppState primaryAppState, AppState globalAppState)
        {
            var appThingType = Target.Target;
            var appStack = this;
            var siteStack = primaryAppState?.SettingsInApp(sp).Get(appThingType);
            var global = globalAppState?.SettingsInApp(sp).Get(appThingType);
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                // App level
                new KeyValuePair<string, IPropertyLookup>(PartApp, appStack.MetadataItem),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, appStack.ForApp),
                // Site level
                new KeyValuePair<string, IPropertyLookup>(PartSite, siteStack?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, siteStack?.ForSite),
                // Global
                new KeyValuePair<string, IPropertyLookup>(PartGlobal, global?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, global?.ForApp ?? global?.ForSite),
                // System Presets
                new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, appThingType == AppThingsToStack.Resources 
                    ? Configuration.Global.SystemResources 
                    : Configuration.Global.SystemSettings)
            };
            return sources;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// All SystemSettings entities - on the content-app, there could be two which are relevant
        /// 1. The one with an empty SettingsEntityScope
        /// </summary>
        private SynchronizedEntityList SysEntitiesInApp => _sysSettingEntities ?? (_sysSettingEntities = MakeSyncListOfType(Target.SystemType));
        private SynchronizedEntityList _sysSettingEntities;

        private SynchronizedEntityList MakeSyncListOfType(string typeName)
            => new SynchronizedEntityList(Parent, () => Parent.Index.Values.Where(e => e.Type.Is(typeName)).ToImmutableArray());


        #endregion

    }
}
