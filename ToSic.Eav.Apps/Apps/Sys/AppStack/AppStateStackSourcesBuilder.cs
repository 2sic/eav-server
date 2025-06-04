using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Sys.Caching;
using ToSic.Sys.Caching.Synchronized;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.Apps.State;

/// <summary>
/// This object creates caches for Settings or Resources
/// It will handle all kinds of thing-lists incl. SystemSettings / SystemResources, Settings, Resources etc.
///
/// Note that it will just build the list, and then exit.
/// The result will remain in the App Piggyback, but without this object and without the Owner, etc.
/// </summary>
[PrivateApi]
internal class AppStateStackSourcesBuilder(
    AppThingsIdentifiers target,
    IAppStateCache owner,
    IAppStateCache ancestorOrNull,
    IAppStateCache siteOrNull,
    IAppStateCache global,
    IAppStateCache preset
)
{
    private IAppStateCache Owner { get; } = owner;

    /// <summary>
    /// Ancestor can be null, as many apps don't have an own ancestor.
    /// </summary>
    private IAppStateCache AncestorOrNull { get; } = ancestorOrNull;

    /// <summary>
    /// Site can be null, if we're getting the stack of the global App, which doesn't have a site-app.
    /// </summary>
    private IAppStateCache SiteOrNull { get; } = siteOrNull;

    private IAppStateCache Global { get; } = global;
    private IAppStateCache Preset { get; } = preset;
    public readonly AppThingsIdentifiers Target = target;

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
        // Since this was bug, we will leave it in 13.03 to better debug
        // But we should probably null it afterwards
        var cachedStack = new SynchronizedObject<List<KeyValuePair<string, IPropertyLookup>>>(cacheExpiry,
            () => RebuildStack(Target.Target, buildLog));

        return l.Return(cachedStack, "built");
    }

    private CacheExpiringMultiSource GetMultiSourceCacheExpiry()
    {
        var cacheExpires = SiteOrNull == null
            ? [Owner, Global]
            : new ITimestamped[] { Owner, SiteOrNull, Global };
        var cacheExpiry = new CacheExpiringMultiSource(cacheExpires);
        return cacheExpiry;
    }


    private List<KeyValuePair<string, IPropertyLookup>> RebuildStack(AppThingsToStack thingType, ILog buildLog = null)
    {
        var l = buildLog.Fn<List<KeyValuePair<string, IPropertyLookup>>>();
        void LogSource(string name, IAppStateMetadata state)
            => l.A($"{name}: {state != null}; MD: {state?.MetadataItem?.EntityId}; CustomItem: {state?.CustomItem?.EntityId}; ScopeAny: {state?.SystemItem?.EntityId};");

        var parts = new List<(string Name, IAppStateCache Source, string KeyCustom, string KeySystem)>()
        {
            ("Owner", Owner, PartApp, PartAppSystem),
            ("Ancestor", AncestorOrNull, PartAncestor, PartAncestorSystem),
            ("Site", SiteOrNull, PartSite, PartSiteSystem),
            ("Global", Global, PartGlobal, PartGlobalSystem),
            ("Preset", Preset, null, PartPresetSystem)
        };

        // Build sources
        var sources = new List<KeyValuePair<string, IPropertyLookup>>();

        foreach (var (name, source, keyCustom, keySystem) in parts)
        {
            var stateMetadata = source?.ThingInApp(thingType);
            LogSource(name, stateMetadata);
            if (stateMetadata == null)
                continue;
            if (keyCustom != null)
                sources.Add(new(keyCustom, stateMetadata.MetadataItem));
            if (keySystem != null)
                sources.Add(new(keySystem, stateMetadata.SystemItem));
        }

        return l.Return(sources, "ok");
    }

    #endregion
}