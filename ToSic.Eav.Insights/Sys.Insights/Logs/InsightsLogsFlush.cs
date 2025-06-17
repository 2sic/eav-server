namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogsFlush(LazySvc<ILogStoreLive> logStore)
    : InsightsProvider(new() { Name = Link, HelpCategory = HiddenFromAutoDisplay, Title = "Flush Logs" }, connect: [logStore])
{
    public static string Link = "LogsFlush";

    public override string HtmlBody()
        => LogsFlush(Key);

    internal string LogsFlush(string key)
    {
        Log.A($"flush log for {key}");
        logStore.Value.FlushSegment(key);
        return $"flushed log history for {key}";
    }
}