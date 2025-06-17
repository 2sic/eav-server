using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.PropertyStack.Sys;
using ToSic.Eav.Data.Sys;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;

namespace ToSic.Eav.Apps.Sys.AppStack;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppDataStackService(IAppReaderFactory appReaders) : ServiceBase("App.Stack", connect: [appReaders])
{
    public AppDataStackService Init(IAppReader appReader)
    {
        AppSpecs = appReader;
        return this;
    }

    public AppDataStackService InitForPrimaryAppOfZone(int zoneId)
    {
        AppSpecs = appReaders.GetZonePrimary(zoneId) ?? throw new NullReferenceException($"Error building stack, can't find zone {zoneId}");
        return this;
    }

    private IAppReader AppSpecs { get; set; } = null!;

    public PropertyStack GetStack(string part, IEntity? viewPart = null)
    {
        var partId = part == RootNameSettings ? Settings : Resources;
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

    private IList<KeyValuePair<string, IPropertyLookup>> GetOrGenerate(AppThingsIdentifiers target)
    {
        var l = Log.Fn<IList<KeyValuePair<string, IPropertyLookup>>>(target.Target.ToString());
        var sources = AppSpecs.GetCache().PiggyBack.GetOrGenerate(
            PiggyBackId + target.Target,
            () => Get(target).FullStack(Log).Where(pair => pair.Value != null).ToListOpt()
        );
        return l.ReturnAndLog(sources!);
    }

    private AppStateStackSourcesBuilder Get(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStackSourcesBuilder>(target.Target.ToString());
        // Site should be skipped on the global zone
        l.A($"Owner: {AppSpecs.Show()}");
        var siteAppReader = AppSpecs.ZoneId == KnownAppsConstants.DefaultZoneId
            ? null
            : appReaders.GetZonePrimary(AppSpecs.ZoneId);
        l.A($"Site: {siteAppReader?.Show()}");
        var global = appReaders.Get(KnownAppsConstants.GlobalIdentity);
        l.A($"Global: {global.Show()}");
        var preset = appReaders.GetSystemPreset();
        l.A($"Preset: {preset.Show()}");

        // Find the ancestor, but only use it if it's _not_ the Preset
        var appState = AppSpecs.GetCache();
        var appAncestor = appState.ParentApp?.AppState;
        var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == KnownAppsConstants.PresetAppId
            ? null
            : appAncestor;
        l.A($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {KnownAppsConstants.PresetAppId}");

        var stackCache = new AppStateStackSourcesBuilder(
            target,
            appState,
            ancestorIfNotPreset,
            siteAppReader?.GetCache(),
            global?.GetCache()!, // added ! because it always worked, but it's unclear why we have a ? in the first place
            preset?.GetCache()!  // added ! because it always worked, but it's unclear why we have a ? in the first place
        );

        return l.ReturnAndLog(stackCache, "created");
    }
}