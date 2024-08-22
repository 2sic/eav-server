namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsGlobalTypes(IAppReaders appStates) : InsightsTypes(appStates, "GlobalTypes")
{
    public override string Title => $"Global {base.Title}";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        var globTypes = AppStates.GetSystemPreset().ContentTypes;
        var msg = TypesTable(Constants.PresetAppId, globTypes, null);

        return l.ReturnAsOk(msg);
    }
}