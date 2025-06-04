namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogsFlush(LazySvc<ILogStoreLive> logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [logStore])
{
    public static string Link = "LogsFlush";

    public override string Title => "Flush Logs";

    public override string HtmlBody()
        => LogsFlush(Key);

    internal string LogsFlush(string key)
    {
        Log.A($"flush log for {key}");
        logStore.Value.FlushSegment(key);
        return $"flushed log history for {key}";
    }
}