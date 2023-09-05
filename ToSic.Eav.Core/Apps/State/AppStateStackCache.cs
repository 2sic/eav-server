using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

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
            var l = buildLog.Fn<List<KeyValuePair<string, IPropertyLookup>>>();
            if (_fullStackSynched != null)
            {
                if (!_fullStackSynched.CacheChanged())
                    return l.Return(_fullStackSynched.Value, "existing");
                l.A("Cache changed, will rebuild");
            }

            _fullStackSynched = BuildCachedStack(buildLog);
            return l.Return(_fullStackSynched.Value, "created");
        }

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> _fullStackSynched;

        private SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack(ILog buildLog)
        {
            var l = buildLog.Fn<SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>>();
            var cacheExpiry = GetMultiSourceCacheExpiry();
            // 2022-03-11 2dm - we're currently including the build log
            // we assume it won't remain in memory, but there is a small risk of a memory leak here
            // Since this was bugg, we will leave it in 13.03 to better debug
            // But we should probably null it afterwards
            var cachedStack = new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(cacheExpiry, 
                () => RebuildStack(buildLog));

            return l.Return(cachedStack, "built");
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
            var l = buildLog.Fn<List<KeyValuePair<string, IPropertyLookup>>>();
            void LogSource(string name, AppStateMetadata state)
                => l.A($"{name}: {state != null}; MD: {state?.MetadataItem?.EntityId}; CustomItem: {state?.CustomItem?.EntityId}; ScopeAny: {state?.SystemItem?.EntityId};");

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
            var sources = new List<KeyValuePair<string, IPropertyLookup>>();
            void AddSource(string name, IPropertyLookup value)
                => sources.Add(new KeyValuePair<string, IPropertyLookup>(name, value));

            AddSource(PartApp, appStack.MetadataItem);
            AddSource(PartAppSystem, appStack.SystemItem);


            // Ancestor Level (only for inherited apps, v13)
            if (ancestorOrNull != null)
            {
                AddSource(PartAncestor, ancestorOrNull.MetadataItem);
                AddSource(PartAncestorSystem, ancestorOrNull.SystemItem);
            }

            // Site level
            if (siteOrNull != null)
            {
                AddSource(PartSite, siteOrNull.CustomItem);
                AddSource(PartSiteSystem, siteOrNull.SystemItem);
            }


            // Global
            if (global != null)
            {
                AddSource(PartGlobal, global.CustomItem);
                AddSource(PartGlobalSystem, global.SystemItem);
            }

            // System Presets
            AddSource(PartPresetSystem, preset.SystemItem);
            return l.Return(sources, "ok");
        }

        #endregion
    }
}
