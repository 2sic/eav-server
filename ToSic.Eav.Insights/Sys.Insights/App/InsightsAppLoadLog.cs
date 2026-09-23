using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Eav.Sys.Insights.Logs;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsAppLoadLog(LazySvc<IAppStateCacheService> appStates, LazySvc<IInsightsLogSnapshotReader> snapshotReader)
    : InsightsProvider(new() { Name = Link, Title = "App Load Log" }, connect: [appStates, snapshotReader])
{
    public static string Link = "AppLoadLog";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, out var message))
            return message;

        Log.A($"debug app-load {AppId}");
        var appLog = appStates.Value.Get(AppId.Value).Log;
        // App-load already owns the exact Legacy log, so do not snapshot the complete retained history here.
        var logHtml = appLog is Log
            ? new InsightsLogsHelper()
            : new InsightsLogsHelper(snapshotReader.Value.Snapshot());
        return InsightsHtmlParts.PageStyles()
               + logHtml.DumpTree(
                   $"2sxc load log for app {AppId}",
                   appLog
               );
    }

}
