using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.Apps;

[PrivateApi]
partial class AppSettingsStack
{
    public PropertyStack GetStack(string part, IEntity viewPart = null)
    {
        var partId = part == RootNameSettings ? Settings : Resources;
        var sources = GetStack(partId);
        return new PropertyStack().Init(part, sources);
    }

    public List<KeyValuePair<string, IPropertyLookup>> GetStack(AppThingsIdentifiers target)
        => GetStack(target, null);

    public List<KeyValuePair<string, IPropertyLookup>> GetStack(AppThingsIdentifiers target, IEntity viewPart)
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

    private AppStateStackCache GetOrGenerate(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStackCache>(target.Target.ToString());
        return l.ReturnAndLog(Reader.PiggyBack.GetOrGenerate(PiggyBackId + target.Target, () => Get(target)));
    }

    private AppStateStackCache Get(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStackCache>(target.Target.ToString());
        // Site should be skipped on the global zone
        l.A($"Owner: {Reader.Show()}");
        var site = Reader.ZoneId == Constants.DefaultZoneId ? null : _appStates.GetPrimaryApp(Reader.ZoneId, Log);
        l.A($"Site: {site?.Show()}");
        var global = _appStates.GetReaderInternalOrNull(Constants.GlobalIdentity);
        l.A($"Global: {global?.Show()}");
        var preset = _appStates.GetPresetApp();
        l.A($"Preset: {preset?.Show()}");

        // Find the ancestor, but only use it if it's not the preset
        var appAncestor = Reader.ParentAppState;
        var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == Constants.PresetAppId ? null : appAncestor;
        l.A($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {Constants.PresetAppId}");

        var stackCache = new AppStateStackCache(Reader.StateCache, ancestorIfNotPreset, site, global?.StateCache, preset, target);

        return l.ReturnAndLog(stackCache, "created");
    }
}