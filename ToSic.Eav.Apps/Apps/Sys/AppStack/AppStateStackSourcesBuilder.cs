using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Sys.Caching;
using ToSic.Sys.Caching.Synchronized;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;

namespace ToSic.Eav.Apps.Sys.AppStack;

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
    IAppStateCache? ancestorOrNull,
    IAppStateCache? siteOrNull,
    IAppStateCache global,
    IAppStateCache preset
)
{
    private IAppStateCache Owner { get; } = owner;

    /// <summary>
    /// Ancestor can be null, as many apps don't have an own ancestor.
    /// </summary>
    private IAppStateCache? AncestorOrNull { get; } = ancestorOrNull;

    /// <summary>
    /// Site can be null, if we're getting the stack of the global App, which doesn't have a site-app.
    /// </summary>
    private IAppStateCache? SiteOrNull { get; } = siteOrNull;

    private IAppStateCache Global { get; } = global;
    private IAppStateCache Preset { get; } = preset;
    public readonly AppThingsIdentifiers Target = target;

    #region Full Stack

    /// <summary>
    /// Get the stack of Settings which applies to this app
    /// </summary>
    /// <returns></returns>
    internal SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>> FullStack(ILog buildLog)
    {
        var l = buildLog.Fn<SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>>>();
        if (_fullStackSynced != null)
        {
            if (!_fullStackSynced.CacheChanged())
                return l.Return(_fullStackSynced, "existing");
            l.A("Cache changed, will rebuild");
        }

        _fullStackSynced = BuildCachedStack(buildLog);
        return l.Return(_fullStackSynced, "created");
    }

    private SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>>? _fullStackSynced;

    private SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>> BuildCachedStack(ILog buildLog)
    {
        var l = buildLog.Fn<SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>>>();

        var cacheExpiry = GetMultiSourceCacheExpiry(buildLog);
        // 2022-03-11 2dm - we're currently including the build log
        // we assume it won't remain in memory, but there is a small risk of a memory leak here
        // Since this was bug, we will leave it in 13.03 to better debug
        // But we should probably set it to null it afterward
        var cachedStack = new SynchronizedObject<ICollection<KeyValuePair<string, IPropertyLookup>>>(
            cacheExpiry,
            () => RebuildStack(Target.Target, buildLog)
        );

        return l.Return(cachedStack, "built");
    }

    private CacheExpiringMultiSource GetMultiSourceCacheExpiry(ILog buildLog)
    {
        var l = buildLog.Fn<CacheExpiringMultiSource>($"WithSite: {SiteOrNull != null}");
        ITimestamped[] cacheExpires = SiteOrNull == null
            ? [Owner, Global]
            : [Owner, SiteOrNull, Global];

        // Log all the timestamps for detailed debugging
        foreach (var timestamped in cacheExpires)
            l.A($"Timestamp: {timestamped.CacheTimestamp}");

        var cacheExpiry = new CacheExpiringMultiSource(cacheExpires);
        return l.Return(cacheExpiry, $"TimeStamp: {cacheExpiry.CacheTimestamp}");
    }


    private ICollection<KeyValuePair<string, IPropertyLookup>> RebuildStack(AppThingsToStack thingType, ILog? buildLog = null)
    {
        var l = buildLog.Fn<ICollection<KeyValuePair<string, IPropertyLookup>>>();

        // List of the 5 segments, each containing up to 2 sources; if all are stacked, it would have max 9 sources
        var parts = new List<(string Name, IAppStateCache? Source, string? KeyCustom, string KeySystem)>
        {
            ("Owner", Owner, PartApp, PartAppSystem),
            ("Ancestor", AncestorOrNull, PartAncestor, PartAncestorSystem),
            ("Site", SiteOrNull, PartSite, PartSiteSystem),
            ("Global", Global, PartGlobal, PartGlobalSystem),
            ("Preset", Preset, null, PartPresetSystem)
        };

        // Build sources
        var sources = new List<KeyValuePair<string, IPropertyLookup?>>();

        foreach (var (name, source, keyCustom, keySystem) in parts)
        {
            var stateMetadata = source?.ThingInApp(thingType);
            LogSource(name, stateMetadata);
            if (stateMetadata == null)
                continue;
            if (keyCustom != null)
                sources.Add(new(keyCustom, stateMetadata.MetadataItem));
            if (keySystem != null!)
                sources.Add(new(keySystem, stateMetadata.SystemItem));
        }

        // Drop all null-values
        var filtered = sources
            .Where(pair => pair.Value != null)
            .ToListOpt();

        return l.Return(filtered! /* nulls were just removed */, "ok");

        void LogSource(string name, IAppStateMetadata? state)
            => l.A($"{name}: {state != null}; MD: {state?.MetadataItem?.EntityId}; CustomItem: {state?.CustomItem?.EntityId}; ScopeAny: {state?.SystemItem?.EntityId};");
    }

    #endregion
}