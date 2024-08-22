using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Services;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.Apps.Services;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppDataStackService(IAppReaders appReaders) : ServiceBase("App.Stack", connect: [appReaders])
{
    public AppDataStackService Init(IHas<IAppSpecsWithStateAndCache> state)
    {
        AppSpecs = state.Value;
        return this;
    }

    public AppDataStackService InitForPrimaryAppOfZone(int zoneId)
    {
        AppSpecs = appReaders.GetPrimaryReader(zoneId, Log);
        return this;
    }

    private IAppSpecsWithStateAndCache AppSpecs { get; set; }

    public PropertyStack GetStack(string part, IEntity viewPart = null)
    {
        var partId = part == RootNameSettings ? Settings : Resources;
        var sources = GetStack(partId, viewPart);
        return new PropertyStack().Init(part, sources);
    }


    public List<KeyValuePair<string, IPropertyLookup>> GetStack(AppThingsIdentifiers target, IEntity viewPart = default)
    {
        var l = Log.Fn<List<KeyValuePair<string, IPropertyLookup>>>(
            $"target: {target.Target}, Has View: {viewPart != null}");
        // "View" Settings/Resources - always add, no matter if null, so the key always exists
        var sources = new List<KeyValuePair<string, IPropertyLookup>>
        {
            new(PartView, viewPart)
        };

        // All in the App and below
        sources.AddRange(GetOrGenerate(target).FullStack(Log));
        return l.Return(sources, $"Has {sources.Count}");
    }

    public const string PiggyBackId = "app-stack-";

    private AppStateStack GetOrGenerate(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStack>(target.Target.ToString());
        return l.ReturnAndLog(AppSpecs.PiggyBack.GetOrGenerate(PiggyBackId + target.Target, () => Get(target)));
    }

    private AppStateStack Get(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStack>(target.Target.ToString());
        // Site should be skipped on the global zone
        l.A($"Owner: {AppSpecs.Show()}");
        var siteAppReader = AppSpecs.ZoneId == Constants.DefaultZoneId
            ? null
            : appReaders.GetPrimaryReader(AppSpecs.ZoneId, Log);
        l.A($"Site: {siteAppReader?.Show()}");
        var global = appReaders.GetReader(Constants.GlobalIdentity);
        l.A($"Global: {global?.Show()}");
        var preset = appReaders.GetPresetReader();
        l.A($"Preset: {preset?.Show()}");

        // Find the ancestor, but only use it if it's not the preset
        var appAncestor = AppSpecs.Cache.ParentApp?.AppState;
        var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == Constants.PresetAppId ? null : appAncestor;
        l.A($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {Constants.PresetAppId}");

        var stackCache = new AppStateStack(AppSpecs.Cache, ancestorIfNotPreset, siteAppReader?.StateCache, global?.StateCache, preset?.StateCache, target);

        return l.ReturnAndLog(stackCache, "created");
    }
}