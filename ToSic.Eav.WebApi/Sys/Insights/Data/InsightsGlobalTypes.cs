namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsGlobalTypes(IAppReaderFactory appReadFac) : InsightsTypes(appReadFac, "GlobalTypes")
{
    public override string Title => $"Global {base.Title}";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        var globTypes = AppReadFac.GetSystemPreset().ContentTypes;
        var msg = TypesTable(Constants.PresetAppId, globTypes, null);

        return l.ReturnAsOk(msg);
    }
}