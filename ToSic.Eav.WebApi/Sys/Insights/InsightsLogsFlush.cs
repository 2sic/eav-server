using ToSic.Eav.Apps.Internal.Insights;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsLogsFlush(ILogStoreLive logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [logStore])
{
    public static string Link = "LogsFlush";

    public override string Title => "Flush Logs";

    public override string HtmlBody()
        => LogsFlush(Key);

    internal string LogsFlush(string key)
    {
        Log.A($"flush log for {key}");
        logStore.FlushSegment(key);
        return $"flushed log history for {key}";
    }
}