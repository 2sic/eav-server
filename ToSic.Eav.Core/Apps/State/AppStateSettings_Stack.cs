using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        /// <summary>
        /// Get the stack of settings which applies to this app
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, IPropertyLookup>> SettingsStackForThisApp()
        {
            if (_settingsStackSynced != null) return _settingsStackSynced.Value;
            
            var primaryAppState = State.Get(State.Identity(Parent.ZoneId, null));
            var globalAppState = State.Get(State.Identity(null, null));

            _settingsStackSynced =
                new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(
                    new CacheExpiringMultiSource(Parent, primaryAppState, globalAppState), () => RebuildSettingsStack(primaryAppState, globalAppState));

            return _settingsStackSynced.Value;
        }

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _settingsStackSynced;

        private List<KeyValuePair<string, IPropertyLookup>> RebuildSettingsStack(AppState primaryAppState, AppState globalAppState)
        {
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                // App level
                new KeyValuePair<string, IPropertyLookup>(PartApp, AppSettings),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, SystemSettings),
                // Site level
                new KeyValuePair<string, IPropertyLookup>(PartSite, primaryAppState?.SettingsInApp.CustomSettings),
                new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, primaryAppState?.SettingsInApp.SystemSettingsEntireSite),
                // Global
                new KeyValuePair<string, IPropertyLookup>(PartGlobal, globalAppState?.SettingsInApp.CustomSettings),
                new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, globalAppState?.SettingsInApp.SystemSettings),
                // System Presets
                new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, Eav.Configuration.Global.SystemSettings)
            };
            return sources;
        }

        public void AddIfNotNull(List<KeyValuePair<string, IPropertyLookup>> sources, string name, IPropertyLookup lookup)
        {
            if (lookup != null) sources.Add(new KeyValuePair<string, IPropertyLookup>(name, lookup));
        }

        
    }
}
