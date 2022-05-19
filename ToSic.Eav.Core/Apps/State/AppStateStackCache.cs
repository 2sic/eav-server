using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
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
        internal AppStateStackCache(AppState owner, AppState ancestor, AppState siteOrNull, AppState global, AppState preset, AppThingsIdentifiers target)
        {
            Owner = owner;
            Ancestor = ancestor;
            SiteOrNull = siteOrNull;
            Global = global;
            Preset = preset;
            Target = target;
        }
        private AppState Owner { get; }
        private AppState Ancestor { get; }

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
        internal List<KeyValuePair<string, IPropertyLookup>> FullStack(ILog buildLog)
        {
            var wrapLog = buildLog.Call<List<KeyValuePair<string, IPropertyLookup>>>();
            if (_fullStackSynched != null)
            {
                if (_fullStackSynched.CacheChanged())
                    buildLog.A("Cache changed, will rebuild");
                else
                    return wrapLog("existing", _fullStackSynched.Value);
            }
            _fullStackSynched = BuildCachedStack(buildLog);
            return wrapLog("created", _fullStackSynched.Value);
        }

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _fullStackSynched;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack(ILog buildLog)
        {
            var wrapLog = buildLog.Call();
            var cacheExpiry = GetMultiSourceCacheExpiry();
            // 2022-03-11 2dm - we're currently including the build log
            // we assume it won't remain in memory, but there is a small risk of a memory leak here
            // Since this was bugg, we will leave it in 13.03 to better debug
            // But we should probably null it afterwards
            var cachedStack = new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(cacheExpiry, 
                () => RebuildStack(buildLog));

            wrapLog("built");
            return cachedStack;
        }

        private CacheExpiringMultiSource GetMultiSourceCacheExpiry()
        {
            var cacheExpires = SiteOrNull == null
                ? new ITimestamped[] { Owner, Global }
                : new ITimestamped[] { Owner, SiteOrNull, Global };
            var cacheExpiry = new CacheExpiringMultiSource(cacheExpires);
            return cacheExpiry;
        }


        private List<KeyValuePair<string, IPropertyLookup>> RebuildStack(ILog buildLog = null)
        {
            var wrapLog = buildLog.Call2<List<KeyValuePair<string, IPropertyLookup>>>();

            void LogSource(string name, AppStateMetadata state) 
                => wrapLog.A($"{name}: {state != null}; MD: {state?.MetadataItem?.EntityId}; CustomItem: {state?.CustomItem?.EntityId}; ScopeAny: {state?.SystemItem?.EntityId};");

            var thingType = Target.Target;
            var appStack = Owner.ThingInApp(thingType);
            LogSource("Owner", appStack);
            //log.SafeAdd($"Owner MD: {appStack.MetadataItem != null}, ScopeAny: {appStack.ScopeAny != null}");
            var ancestorOrNull = Ancestor?.ThingInApp(thingType);
            LogSource("Ancestor", ancestorOrNull);
            //log.SafeAdd($"Ancestor: {ancestorOrNull != null}");
            var siteOrNull = SiteOrNull?.ThingInApp(thingType);
            LogSource("Site", siteOrNull);
            //log.SafeAdd($"Site: {siteOrNull != null}");
            var global = Global?.ThingInApp(thingType);
            LogSource("Global", global);
            //log.SafeAdd($"Global: {global != null}");
            var preset = Preset.ThingInApp(thingType);
            LogSource("Preset", preset);
            //log.SafeAdd($"Preset: {preset != null}");

            // Current App level
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                new KeyValuePair<string, IPropertyLookup>(PartApp, appStack.MetadataItem),
                new KeyValuePair<string, IPropertyLookup>(PartAppSystem, appStack.SystemItem),
            };

            // Ancestor Level (only for inherited apps, v13)
            if (ancestorOrNull != null)
            {
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartAncestor, ancestorOrNull.MetadataItem));
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartAncestorSystem, ancestorOrNull.SystemItem));
            }

            // Site level
            if (siteOrNull != null)
            {
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartSite, siteOrNull.CustomItem));
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartSiteSystem, siteOrNull.SystemItem));
            }


            // Global
            if (global != null)
            {
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartGlobal, global.CustomItem));
                sources.Add(new KeyValuePair<string, IPropertyLookup>(PartGlobalSystem, global.SystemItem));
            }

            // System Presets
            sources.Add(new KeyValuePair<string, IPropertyLookup>(PartPresetSystem, preset.SystemItem));
            return wrapLog.Return(sources, "ok");
        }

        #endregion
    }
}
