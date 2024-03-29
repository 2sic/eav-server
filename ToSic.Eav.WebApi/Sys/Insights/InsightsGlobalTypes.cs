﻿namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsGlobalTypes(IAppStates appStates) : InsightsTypes(appStates, "GlobalTypes")
{
    public override string Title => $"Global {base.Title}";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>();
        var globTypes = AppStates.GetPresetReader().ContentTypes;
        var msg = TypesTable(Constants.PresetAppId, globTypes, null);

        return l.ReturnAsOk(msg);
    }
}