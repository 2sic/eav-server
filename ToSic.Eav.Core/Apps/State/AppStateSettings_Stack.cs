using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        public List<KeyValuePair<string, IPropertyLookup>> GetStack(bool forSettings, IEntity viewPart)
        {
            // View level - always add, no matter if null
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                new KeyValuePair<string, IPropertyLookup>(PartView, viewPart)
            };

            // All in the App and below
            sources.AddRange(forSettings ? SettingsStackForThisApp() : ResourcesStackForThisApp());
            return sources;
        }

        /// <summary>
        /// Get the stack of Settings which applies to this app
        /// </summary>
        /// <returns></returns>
        private List<KeyValuePair<string, IPropertyLookup>> SettingsStackForThisApp() 
            => (_settingsStackSynced ?? ( _settingsStackSynced = BuildCachedStack(true))).Value;
        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _settingsStackSynced;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack(bool forSettings)
        {
            var primaryAppState = State.Get(State.Identity(Parent.ZoneId, null));
            var globalAppState = State.Get(State.Identity(null, null));

            var cachedStack =
                new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(
                    new CacheExpiringMultiSource(Parent, primaryAppState, globalAppState),
                    () => forSettings ? RebuildStack(primaryAppState, globalAppState, AppThingsToStack.Settings)
                        : RebuildStack(primaryAppState, globalAppState, AppThingsToStack.Resources));
            return cachedStack;
        }


        private List<KeyValuePair<string, IPropertyLookup>> RebuildStack(AppState primaryAppState, AppState globalAppState, AppThingsToStack appThingType)
        {
            var appStack = Get(appThingType);
            var siteStack = primaryAppState?.SettingsInApp.Get(appThingType);
            var global = globalAppState?.SettingsInApp.Get(appThingType);
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                // App level
                new KeyValuePair<string, IPropertyLookup>(PartApp, appStack.AppItem),// appLevel),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, appStack.ForApp),// SettingsStackCache.ForApp),// SystemSettingsForApp),
                // Site level
                new KeyValuePair<string, IPropertyLookup>(PartSite, siteStack?.Custom),// .SettingsStackCache.Custom),// .CustomSettings),
                new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, siteStack?.ForSite),// .SettingsStackCache.ForSite), //.SystemSettingsForSite),
                // Global
                new KeyValuePair<string, IPropertyLookup>(PartGlobal, global?.Custom),// .SettingsStackCache.Custom),// .CustomSettings),
                new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, global?.ForApp),// .SettingsStackCache.ForApp),//.SystemSettingsForApp),
                // System Presets
                new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, Configuration.Global.SystemSettings)
            };
            return sources;
        }

        #region Resources

        /// <summary>
        /// Get the stack of Resources which applies to this app
        /// </summary>
        /// <returns></returns>
        private List<KeyValuePair<string, IPropertyLookup>> ResourcesStackForThisApp() 
            => (_resourcesStackSynced ?? (_resourcesStackSynced = BuildCachedStack(false))).Value;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _resourcesStackSynced;
        
        #endregion
    }
}
