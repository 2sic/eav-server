using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
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
        internal AppStateStackCache(AppState parent, AppState site, AppState global, AppState preset, AppThingsIdentifiers target)
        {
            Parent = parent;
            Site = site;
            Global = global;
            Preset = preset;
            Target = target;
        }
        private AppState Parent { get; }
        private AppState Site { get; }
        private AppState Global { get; }
        private AppState Preset { get; }
        public readonly AppThingsIdentifiers Target;

        #region Full Stack

        /// <summary>
        /// Get the stack of Settings which applies to this app
        /// </summary>
        /// <returns></returns>
        internal List<KeyValuePair<string, IPropertyLookup>> FullStack()
            => (_fullStackSynched ?? (_fullStackSynched = BuildCachedStack())).Value;
        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _fullStackSynched;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack()
        {
            var cachedStack =
                new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(
                    new CacheExpiringMultiSource(Parent, Site, Global),
                    () => RebuildStack(Site, Global, Preset));
            return cachedStack;
        }


        private List<KeyValuePair<string, IPropertyLookup>> RebuildStack(AppState siteApp, AppState globalApp, AppState presetApp)
        {
            var appThingType = Target.Target;
            var appStack = Parent.ThingInApp(appThingType);
            var siteStack = siteApp?.ThingInApp(appThingType);
            var global = globalApp?.ThingInApp(appThingType);
            var preset = presetApp?.ThingInApp(appThingType);

            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                // App level
                new KeyValuePair<string, IPropertyLookup>(PartApp, appStack.MetadataItem),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, appStack.ScopeAny),
                // Site level
                new KeyValuePair<string, IPropertyLookup>(PartSite, siteStack?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, siteStack?.ScopeAny),
                // Global
                new KeyValuePair<string, IPropertyLookup>(PartGlobal, global?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, global?.ScopeAny),
                // System Presets
                new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, preset?.ScopeAny)

                //new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, appThingType == AppThingsToStack.Resources 
                //    ? Configuration.Global.SystemResources 
                //    : Configuration.Global.SystemSettings)
            };
            return sources;
        }

        #endregion
    }
}
