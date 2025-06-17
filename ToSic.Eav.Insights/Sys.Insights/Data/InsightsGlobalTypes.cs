using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsGlobalTypes(IAppReaderFactory appReadFac) : InsightsTypes(appReadFac, "GlobalTypes")
{
    [field: AllowNull, MaybeNull]
    public override InsightsProviderSpecs Specs => field ??= base.Specs with { Title = $"Global {base.Specs.Title}" };

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        var globTypes = AppReadFac.GetSystemPreset()!.ContentTypes;
        var msg = TypesTable(KnownAppsConstants.PresetAppId, globTypes, []);

        return l.ReturnAsOk(msg);
    }
}