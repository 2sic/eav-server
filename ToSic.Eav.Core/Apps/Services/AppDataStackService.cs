﻿using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Services;
using static ToSic.Eav.Apps.AppStackConstants;

namespace ToSic.Eav.Apps.Services;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppDataStackService(IAppStates appStates) : ServiceBase("App.Stack")
{
    public AppDataStackService Init(IAppState state)
    {
        Reader = (IAppStateInternal)state;
        return this;
    }

    private IAppStateInternal Reader { get; set; }

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
        return l.ReturnAndLog(Reader.PiggyBack.GetOrGenerate(PiggyBackId + target.Target, () => Get(target)));
    }

    private AppStateStack Get(AppThingsIdentifiers target)
    {
        var l = Log.Fn<AppStateStack>(target.Target.ToString());
        // Site should be skipped on the global zone
        l.A($"Owner: {Reader.Show()}");
        var site = Reader.ZoneId == Constants.DefaultZoneId ? null : appStates.GetPrimaryReader(Reader.ZoneId, Log);
        l.A($"Site: {site?.Show()}");
        var global = appStates.GetReader(Constants.GlobalIdentity);
        l.A($"Global: {global?.Show()}");
        var preset = appStates.GetPresetReader();
        l.A($"Preset: {preset?.Show()}");

        // Find the ancestor, but only use it if it's not the preset
        var appAncestor = Reader.ParentAppState;
        var ancestorIfNotPreset = appAncestor == null || appAncestor.AppId == Constants.PresetAppId ? null : appAncestor;
        l.A($"Ancestor: {appAncestor?.Show()} - use: {ancestorIfNotPreset} (won't use if ancestor is preset App {Constants.PresetAppId}");

        var stackCache = new AppStateStack(Reader.StateCache, ancestorIfNotPreset, site?.StateCache, global?.StateCache, preset?.StateCache, target);

        return l.ReturnAndLog(stackCache, "created");
    }
}