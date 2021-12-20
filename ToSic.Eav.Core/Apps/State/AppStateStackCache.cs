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
        internal AppStateStackCache(AppState owner, AppState siteOrNull, AppState global, AppState preset, AppThingsIdentifiers target)
        {
            Owner = owner;
            SiteOrNull = siteOrNull;
            Global = global;
            Preset = preset;
            Target = target;
        }
        private AppState Owner { get; }

        /// <summary>
        /// Site can be null, if we're on the global App, which doesn't have a site-app...
        /// </summary>
        private AppState SiteOrNull { get; }
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
            var cacheExpires = SiteOrNull == null
                ? new ICacheExpiring[] { Owner, Global }
                : new ICacheExpiring[] { Owner, SiteOrNull, Global };
            var cachedStack =
                new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(
                    new CacheExpiringMultiSource(cacheExpires), RebuildStack);
            return cachedStack;
        }


        private List<KeyValuePair<string, IPropertyLookup>> RebuildStack()
        {
            var thingType = Target.Target;
            var appStack = Owner.ThingInApp(thingType);
            var siteOrNull = SiteOrNull?.ThingInApp(thingType);
            var global = Global?.ThingInApp(thingType);
            var preset = Preset.ThingInApp(thingType);

            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                // App level
                new KeyValuePair<string, IPropertyLookup>(PartApp, appStack.MetadataItem),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, appStack.ScopeAny),

                // Site level
                new KeyValuePair<string, IPropertyLookup>(PartSite, siteOrNull?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, siteOrNull?.ScopeAny),

                // Global
                new KeyValuePair<string, IPropertyLookup>(PartGlobal, global?.Custom),
                new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, global?.ScopeAny),

                // System Presets
                new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, preset.ScopeAny)

                //new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, appThingType == AppThingsToStack.Resources 
                //    ? Configuration.Global.SystemResources 
                //    : Configuration.Global.SystemSettings)
            };
            return sources;
        }

        #endregion
    }
}
