using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.Sys.PropertyStack;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;

namespace ToSic.Eav.Apps.Sys.AppStack;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppDataStackService(IAppReaderFactory appReaderFactory/*, ILogStore wipLogStore*/) : ServiceBase("App.Stack", connect: [appReaderFactory])
{
    public AppDataStackService Init(IAppReader appReader)
    {
        AppReader = appReader;

        //wipLogStore.Add("wip", Log);

        return this;
    }

    public AppDataStackService InitForPrimaryAppOfZone(int zoneId)
    {
        AppReader = appReaderFactory.GetZonePrimary(zoneId) ?? throw new NullReferenceException($"Error building stack, can't find zone {zoneId}");
        return this;
    }

    private IAppReader AppReader { get; set; } = null!;

    public PropertyStack GetStack(string part, IEntity? viewPart = null)
    {
        var partId = part == RootNameSettings
            ? Settings
            : Resources;
        var sources = GetStack(partId, viewPart);
        return new PropertyStack().Init(part, sources);
    }

    public List<KeyValuePair<string, IPropertyLookup>> GetStack(AppThingsIdentifiers target, IEntity? viewPart = default)
    {
        var l = Log.Fn<List<KeyValuePair<string, IPropertyLookup>>>(
            $"target: {target.Target}, Has View: {viewPart != null}");
        // "View" Settings/Resources - always add, no matter if null, so the key always exists
        var sources = new List<KeyValuePair<string, IPropertyLookup>>();

        if (viewPart != null)
            sources.Add(new(PartView, viewPart));

        // All in the App and below

        sources.AddRange(GetOrGenerate(target));

        return l.Return(sources, $"Has {sources.Count}");
    }

    public const string PiggyBackId = "app-stack-";

    private ICollection<KeyValuePair<string, IPropertyLookup>> GetOrGenerate(AppThingsIdentifiers target)
    {
        var l = Log.Fn<ICollection<KeyValuePair<string, IPropertyLookup>>>(target.Target.ToString());
        var appState = AppReader.GetCache();

        // Note: the PiggyBack _must_ store the synchronized object, and NOT some List<...> with the resulting data
        // Only then will a future access get the correct items.
        // Before v20, it did some more data filtering and stored a reduced list
        // but that resulting in disabling the SynchronizedObject, so changes often didn't get cache-busted anymore.
        var sources = appState.PiggyBack.GetOrGenerate(
            $"{PiggyBackId}{target.Target}",
            () => Create(target).FullStack(Log)
        );
        return l.Return(sources.Value, $"Expired: {sources.CacheChanged()}; Timestamp: {sources.CacheTimestamp}");
    }

    private AppStateStackSourcesBuilder Create(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStackSourcesBuilder>(target.Target.ToString());
        // Site should be skipped on the global zone
        l.A($"Owner: {AppReader.Show()}");
        var siteAppReader = AppReader.ZoneId == KnownAppsConstants.DefaultZoneId
            ? null
            : appReaderFactory.GetZonePrimary(AppReader.ZoneId);
        l.A($"Site: {siteAppReader?.Show()}");
        var global = appReaderFactory.Get(KnownAppsConstants.GlobalIdentity);
        l.A($"Global: {global.Show()}");
        var preset = appReaderFactory.GetSystemPreset();
        l.A($"Preset: {preset.Show()}");

        // Find the ancestor, but only use it if it's _not_ the Preset
        var appState = AppReader.GetCache();
        var appAncestor = appState.ParentApp.AppState;
        var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == KnownAppsConstants.PresetAppId
            ? null
            : appAncestor;
        l.A($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {KnownAppsConstants.PresetAppId}");

        var stackCache = new AppStateStackSourcesBuilder(
            target,
            appState,
            ancestorIfNotPreset,
            siteAppReader?.GetCache(),
            global.GetCache(), // added ! because it always worked, but it's unclear why we have a ? in the first place
            preset.GetCache()  // added ! because it always worked, but it's unclear why we have a ? in the first place
        );

        return l.ReturnAndLog(stackCache, "created");
    }
}